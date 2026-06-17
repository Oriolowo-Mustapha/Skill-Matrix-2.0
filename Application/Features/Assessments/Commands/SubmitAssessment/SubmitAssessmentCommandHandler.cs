using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;
using MediatR;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Assessments.Commands.SubmitAssessment
{
	public class SubmitAssessmentCommandHandler : IRequestHandler<SubmitAssessmentCommand, AssessmentResultDTO>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAiService _aiService;

		public SubmitAssessmentCommandHandler(IUnitOfWork unitOfWork, IAiService aiService)
		{
			_unitOfWork = unitOfWork;
			_aiService = aiService;
		}

		public async Task<AssessmentResultDTO> Handle(SubmitAssessmentCommand request, CancellationToken cancellationToken)
		{
			var batch = await _unitOfWork.AssessmentBatches.GetBatchForGradingAsync(request.requestDto.AssessmentBatchId);

			if (batch == null)
			{
				throw new NotFoundException("Assessment Batch", request.requestDto.AssessmentBatchId);
			}

			if (batch.AssessmentStatus == AssessmentStatus.Completed)
			{
				throw new AssessmentAlreadyCompletedException();
			}

			// Timer enforcement with 1-minute grace period
			if (batch.StartedAt.HasValue && batch.TimeLimitMinutes.HasValue)
			{
				var deadline = batch.StartedAt.Value.AddMinutes(batch.TimeLimitMinutes.Value).AddMinutes(1);
				if (DateTime.UtcNow > deadline)
				{
					throw new BadRequestException("Assessment time limit exceeded. Your submission was not accepted in time.");
				}
			}

			int correctAnswers = 0;
			int unansweredCount = 0;
			int totalQuestions = batch.Assessments.Count;
			var userResponses = new List<UserResponse>();
			var conceptScores = new Dictionary<string, (int Correct, int Total)>(StringComparer.OrdinalIgnoreCase);

			foreach (var question in batch.Assessments)
			{
				var answerDto = request.requestDto.UserAnswers.FirstOrDefault(a => a.AssessmentQuestionId == question.Id);
				bool isCorrect = false;
				bool isAnswered = answerDto != null;

				if (isAnswered)
				{
					int selectedOptionId = answerDto!.SelectedOptionId;

					// Grade based on question type
					if (question.QuestionType == QuestionType.Coding)
					{
						// For coding questions, the grading happens via the run-code endpoint.
						// The frontend sends SelectedOptionId = -1 for passed, 0 for failed.
						isCorrect = selectedOptionId == -1;
					}
					else
					{
						var selectedOption = question.AssessmentOptions.FirstOrDefault(o => o.Id == selectedOptionId);
						if (selectedOption != null && selectedOption.OptionText == question.CorrectAnswer)
						{
							isCorrect = true;
						}
					}

					var response = new UserResponse
					{
						AssessmentBatchId = batch.Id,
						AssessmentQuestionId = question.Id,
						SelectedOptionId = selectedOptionId,
						Timestamp = DateTime.UtcNow,
						IsCorrect = isCorrect
					};

					if (request.UserRole == Roles.Learner.ToString())
						response.LearnerId = request.UserId;
					else
						response.TeamMemberId = request.UserId;

					userResponses.Add(response);
				}
				else
				{
					unansweredCount++;
				}

				if (isCorrect) correctAnswers++;

				// Concept/Subtopic Aggregation (Edge Case: handle missing/empty concepts)
				string concept = string.IsNullOrWhiteSpace(question.Concept) ? "General Theory" : question.Concept.Trim();
				if (!conceptScores.ContainsKey(concept))
				{
					conceptScores[concept] = (0, 0);
				}
				var current = conceptScores[concept];
				conceptScores[concept] = (current.Correct + (isCorrect ? 1 : 0), current.Total + 1);
			}

			int score = totalQuestions > 0 ? (int)((double)correctAnswers / totalQuestions * 100) : 0;

			// Determine passing threshold based on proficiency level
			int passingScore = GetPassingThreshold(batch.AssignedSkill.ProficiencyLevel);
			bool passed = score >= passingScore;

			var result = new AssessmentResult
			{
				AssessmentBatchId = batch.Id,
				SkillId = batch.SkillId,
				TotalQuestions = totalQuestions,
				NoOfCorrectAnswers = correctAnswers,
				NoOfWrongAnswers = totalQuestions - correctAnswers,
				NoOfUnansweredQuestions = unansweredCount,
				Score = score,
				ProficiencyLevel = batch.AssignedSkill.ProficiencyLevel,
				DateCreated = DateTime.UtcNow,
				Skill = batch.AssignedSkill
			};

			if (request.UserRole == Roles.Learner.ToString())
				result.LearnerID = request.UserId;
			else
				result.TeamMemberID = request.UserId;

			batch.AssessmentStatus = AssessmentStatus.Completed;

			await _unitOfWork.UserResponses.AddRangeAsync(userResponses);
			await _unitOfWork.AssessmentResults.AddAsync(result);
			await _unitOfWork.AssessmentBatches.UpdateAsync(batch);

			// Level Promotion and Badges
			bool levelUp = false;
			ProficiencyLevel newLevel = batch.AssignedSkill.ProficiencyLevel;
			bool badgeUnlocked = false;
			string badgeTitle = string.Empty;

			if (passed)
			{
				string targetBadgeLevel;
				if (batch.AssignedSkill.ProficiencyLevel != ProficiencyLevel.Expert)
				{
					levelUp = true;
					newLevel = batch.AssignedSkill.ProficiencyLevel switch
					{
						ProficiencyLevel.Novice => ProficiencyLevel.Begineer,
						ProficiencyLevel.Begineer => ProficiencyLevel.Intermediate,
						ProficiencyLevel.Intermediate => ProficiencyLevel.Proficient,
						ProficiencyLevel.Proficient => ProficiencyLevel.Expert,
						_ => batch.AssignedSkill.ProficiencyLevel
					};

					batch.AssignedSkill.ProficiencyLevel = newLevel;
					await _unitOfWork.AssignedSkills.UpdateAsync(batch.AssignedSkill);
					targetBadgeLevel = newLevel.ToString();
				}
				else
				{
					batch.AssignedSkill.IsFullyMastered = true;
					await _unitOfWork.AssignedSkills.UpdateAsync(batch.AssignedSkill);
					targetBadgeLevel = "Master";
				}

				// Award Milestone Badges (Edge Case: prevent duplicate badge awards)
				var badgesList = await _unitOfWork.Badges.FindAsync(
					b => b.ProficiencyLevel.Equals(targetBadgeLevel, StringComparison.OrdinalIgnoreCase)
				);
				var milestoneBadge = badgesList.FirstOrDefault();

				if (milestoneBadge != null)
				{
					bool alreadyEarned = false;
					if (request.UserRole == Roles.Learner.ToString())
					{
						alreadyEarned = await _unitOfWork.AssignedBadges.ExistsAsync(ab => ab.LearnerID == request.UserId && ab.BadgeId == milestoneBadge.Id);
					}
					else
					{
						alreadyEarned = await _unitOfWork.AssignedBadges.ExistsAsync(ab => ab.TeamMemberId == request.UserId && ab.BadgeId == milestoneBadge.Id);
					}

					if (!alreadyEarned)
					{
						var assignedBadge = new AssignedBadge
						{
							BadgeId = milestoneBadge.Id,
							DateAwarded = DateTime.UtcNow
						};

						if (request.UserRole == Roles.Learner.ToString())
							assignedBadge.LearnerID = request.UserId;
						else
							assignedBadge.TeamMemberId = request.UserId;

						await _unitOfWork.AssignedBadges.AddAsync(assignedBadge);
						badgeUnlocked = true;
						badgeTitle = milestoneBadge.Name;
					}
				}
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			// Identify Gaps and update/resolve existing active gaps (Edge Case: resolve duplicates)
			var skillGaps = new List<SkillGap>();
			foreach (var kvp in conceptScores)
			{
				string conceptName = kvp.Key;
				var stats = kvp.Value;
				int conceptScore = stats.Total > 0 ? (int)((double)stats.Correct / stats.Total * 100) : 0;

				// Threshold for competence is 70%
				if (conceptScore < 70)
				{
					var gap = new SkillGap
					{
						SkillId = batch.SkillId,
						AssessmentResultId = result.Id,
						Concept = conceptName,
						Score = conceptScore,
						DateIdentified = DateTime.UtcNow,
						Status = "Active"
					};

					if (request.UserRole == Roles.Learner.ToString())
						gap.LearnerId = request.UserId;
					else
						gap.TeamMemberId = request.UserId;

					skillGaps.Add(gap);
				}
			}

			// Clean up previous active gaps for this user and skill
			var existingGapsList = await _unitOfWork.SkillGaps.GetAllAsync();
			var userGaps = existingGapsList.Where(g =>
				g.SkillId == batch.SkillId &&
				g.Status == "Active" &&
				(request.UserRole == Roles.Learner.ToString() ? g.LearnerId == request.UserId : g.TeamMemberId == request.UserId)
			).ToList();

			foreach (var oldGap in userGaps)
			{
				oldGap.Status = "Resolved";
				await _unitOfWork.SkillGaps.UpdateAsync(oldGap);
			}

			if (skillGaps.Any())
			{
				await _unitOfWork.SkillGaps.AddRangeAsync(skillGaps);
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			// Generate Tailored Improvement Plan (passing active gaps)
			var improvementPlan = await _aiService.GenerateImprovementPlanAsync(result, skillGaps);
			improvementPlan.AssessmentResultId = result.Id;
			await _unitOfWork.ImprovementPlans.AddAsync(improvementPlan);
			
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return new AssessmentResultDTO
			{
				Id = result.Id,
				SkillName = batch.AssignedSkill.Name,
				Score = result.Score,
				NoOfCorrectAnswers = result.NoOfCorrectAnswers,
				NoOfWrongAnswers = result.NoOfWrongAnswers,
				TotalQuestions = result.TotalQuestions,
				ProficiencyLevel = result.ProficiencyLevel.ToString(),
				DateCompleted = result.DateCreated,
				Passed = passed,
				PassingScore = passingScore,
				LevelUp = levelUp,
				NewProficiencyLevel = newLevel.ToString(),
				BadgeUnlocked = badgeUnlocked,
				BadgeTitle = badgeTitle
			};
		}

		private static int GetPassingThreshold(ProficiencyLevel level)
		{
			return level switch
			{
				ProficiencyLevel.Novice => 50,
				ProficiencyLevel.Begineer => 60,
				ProficiencyLevel.Intermediate => 70,
				ProficiencyLevel.Proficient => 80,
				ProficiencyLevel.Expert => 90,
				_ => 70
			};
		}
	}
}

using Application.DTOs;
using Application.DTOs.Assessments;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Assessments.Commands.SubmitAssessment
{
	public class SubmitAssessmentCommandHandler : IRequestHandler<SubmitAssessmentCommand, BaseResponse<AssessmentResultDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAiService _aiService;
		private readonly ICodeExecutionService _codeExecutionService;

		public SubmitAssessmentCommandHandler(IUnitOfWork unitOfWork, IAiService aiService, ICodeExecutionService codeExecutionService)
		{
			_unitOfWork = unitOfWork;
			_aiService = aiService;
			_codeExecutionService = codeExecutionService;
		}

		public async Task<BaseResponse<AssessmentResultDTO>> Handle(SubmitAssessmentCommand request, CancellationToken cancellationToken)
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

			// Timer deadline check (log warning if submitted past deadline, but gracefully grade submitted answers)
			if (batch.StartedAt.HasValue && batch.TimeLimitMinutes.HasValue)
			{
				var deadline = batch.StartedAt.Value.AddMinutes(batch.TimeLimitMinutes.Value).AddMinutes(2);
				if (DateTime.UtcNow > deadline)
				{
					System.Diagnostics.Debug.WriteLine($"[SubmitAssessment] Batch {batch.Id} submitted after timer deadline. Gracefully processing submitted answers.");
				}
			}

			// Load existing incrementally saved user responses from DB
			var existingDbResponses = (await _unitOfWork.UserResponses.FindAsync(ur => ur.AssessmentBatchId == batch.Id)).ToList();
			var inputAnswers = request.requestDto?.UserAnswers ?? new List<UserAnswerDTO>();

			// Merge input answers with existing DB responses (input answers take precedence if provided)
			var effectiveAnswers = new List<UserAnswerDTO>();
			foreach (var q in batch.Assessments)
			{
				var fromInput = inputAnswers.FirstOrDefault(a => a.AssessmentQuestionId == q.Id);
				if (fromInput != null && (fromInput.SelectedOptionId.HasValue || !string.IsNullOrWhiteSpace(fromInput.SubmittedCode)))
				{
					effectiveAnswers.Add(fromInput);
				}
				else
				{
					var fromDb = existingDbResponses.FirstOrDefault(r => r.AssessmentQuestionId == q.Id);
					if (fromDb != null)
					{
						effectiveAnswers.Add(new UserAnswerDTO
						{
							AssessmentQuestionId = fromDb.AssessmentQuestionId,
							SelectedOptionId = fromDb.SelectedOptionId,
							SubmittedCode = fromDb.SubmittedCode
						});
					}
				}
			}

			// Parallel Execution for Coding Questions
			var codingTasks = new List<(Assessment Question, UserAnswerDTO Answer, Task<CodeExecutionResponseDTO> Task)>();
			foreach (var question in batch.Assessments.Where(q => q.QuestionType == QuestionType.Coding))
			{
				var answerDto = effectiveAnswers.FirstOrDefault(a => a.AssessmentQuestionId == question.Id);
				if (answerDto != null && !string.IsNullOrWhiteSpace(answerDto.SubmittedCode))
				{
					var lang = string.IsNullOrWhiteSpace(question.CorrectAnswer) ? "csharp" : question.CorrectAnswer.Trim();
					var executionRequest = new CodeExecutionRequestDTO
					{
						Language = lang,
						SourceCode = answerDto.SubmittedCode,
						ExpectedOutput = question.ExpectedOutput ?? string.Empty
					};
					var task = _codeExecutionService.ExecuteCodeAsync(executionRequest);
					codingTasks.Add((question, answerDto, task));
				}
			}

			if (codingTasks.Any())
			{
				await Task.WhenAll(codingTasks.Select(t => t.Task));
			}

			var codingResults = codingTasks.ToDictionary(
				t => t.Question.Id,
				t => t.Task.Result
			);

			int correctAnswers = 0;
			int unansweredCount = 0;
			int totalQuestions = batch.Assessments.Count;
			var conceptScores = new Dictionary<string, (int Correct, int Total)>(StringComparer.OrdinalIgnoreCase);

			foreach (var question in batch.Assessments)
			{
				var answerDto = effectiveAnswers.FirstOrDefault(a => a.AssessmentQuestionId == question.Id);
				bool isCorrect = false;
				bool isAnswered = answerDto != null;

				if (isAnswered)
				{
					int? selectedOptionId = (answerDto?.SelectedOptionId.HasValue == true && answerDto.SelectedOptionId.Value > 0) 
						? answerDto.SelectedOptionId.Value 
						: null;

					// Grade based on question type
					if (question.QuestionType == QuestionType.Coding)
					{
						selectedOptionId = null;
						if (codingResults.TryGetValue(question.Id, out var executionResult))
						{
							isCorrect = executionResult.IsSuccess;
						}
						else
						{
							isCorrect = false;
						}
					}
					else
					{
						if (selectedOptionId.HasValue)
						{
							var selectedOption = question.AssessmentOptions.FirstOrDefault(o => o.Id == selectedOptionId.Value);
							if (selectedOption != null && selectedOption.OptionText == question.CorrectAnswer)
							{
								isCorrect = true;
							}
						}
					}

					var existingResponse = existingDbResponses.FirstOrDefault(r => r.AssessmentQuestionId == question.Id);
					if (existingResponse != null)
					{
						existingResponse.SelectedOptionId = selectedOptionId;
						existingResponse.SubmittedCode = answerDto?.SubmittedCode;
						existingResponse.IsCorrect = isCorrect;
						existingResponse.UpdatedAt = DateTime.UtcNow;
						await _unitOfWork.UserResponses.UpdateAsync(existingResponse);
					}
					else
					{
						var newResp = new UserResponse
						{
							AssessmentBatchId = batch.Id,
							AssessmentQuestionId = question.Id,
							SelectedOptionId = selectedOptionId,
							Timestamp = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow,
							IsCorrect = isCorrect,
							SubmittedCode = answerDto?.SubmittedCode
						};
						if (request.UserRole == Roles.Learner.ToString())
							newResp.LearnerId = request.UserId;
						else
							newResp.TeamMemberId = request.UserId;

						await _unitOfWork.UserResponses.AddAsync(newResp);
					}
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
				DateModified = DateTime.UtcNow,
				Skill = batch.AssignedSkill
			};

			if (request.UserRole == Roles.Learner.ToString())
				result.LearnerID = request.UserId;
			else
				result.TeamMemberID = request.UserId;

			batch.AssessmentStatus = AssessmentStatus.Completed;

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

			var responseDto = new AssessmentResultDTO
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

			return BaseResponse<AssessmentResultDTO>.SuccessResponse(responseDto, "Assessment submitted and graded successfully.");
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

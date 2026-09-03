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
		private readonly IActivityLogService _activityLogService;

		public SubmitAssessmentCommandHandler(IUnitOfWork unitOfWork, IAiService aiService, ICodeExecutionService codeExecutionService, IActivityLogService activityLogService)
		{
			_unitOfWork = unitOfWork;
			_aiService = aiService;
			_codeExecutionService = codeExecutionService;
			_activityLogService = activityLogService;
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
			int totalMcqs = 0;
			int correctMcqs = 0;
			int totalCodingQuestions = 0;
			int totalTestCases = 0;
			int totalPassedTestCases = 0;
			var conceptScores = new Dictionary<string, (int Correct, int Total)>(StringComparer.OrdinalIgnoreCase);

			foreach (var question in batch.Assessments)
			{
				var answerDto = effectiveAnswers.FirstOrDefault(a => a.AssessmentQuestionId == question.Id);
				bool isCorrect = false;
				bool isAnswered = answerDto != null;

				if (question.QuestionType == QuestionType.Coding)
				{
					totalCodingQuestions++;
				}
				else
				{
					totalMcqs++;
				}

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
							if (executionResult.TotalCount > 0)
							{
								totalTestCases += executionResult.TotalCount;
								totalPassedTestCases += executionResult.PassedCount;
								isCorrect = executionResult.IsSuccess || (executionResult.PassedCount == executionResult.TotalCount);
							}
							else
							{
								totalTestCases += 1;
								totalPassedTestCases += executionResult.IsSuccess ? 1 : 0;
								isCorrect = executionResult.IsSuccess;
							}
						}
						else
						{
							totalTestCases += 1;
							isCorrect = false;
						}
					}
					else
					{
						if (selectedOptionId.HasValue)
						{
							var selectedOption = question.AssessmentOptions.FirstOrDefault(o => o.Id == selectedOptionId.Value);
							if (selectedOption != null && string.Equals(selectedOption.OptionText?.Trim(), question.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase))
							{
								isCorrect = true;
								correctMcqs++;
							}
						}
					}

					string? consoleOutput = null;
					string? executionResultsJson = null;
					if (question.QuestionType == QuestionType.Coding && codingResults.TryGetValue(question.Id, out var execRes))
					{
						consoleOutput = execRes?.ConsoleOutput;
						if (execRes?.TestResults != null && execRes.TestResults.Any())
						{
							executionResultsJson = System.Text.Json.JsonSerializer.Serialize(execRes.TestResults);
						}
					}

					var existingResponse = existingDbResponses.FirstOrDefault(r => r.AssessmentQuestionId == question.Id);
					if (existingResponse != null)
					{
						existingResponse.SelectedOptionId = selectedOptionId;
						existingResponse.SubmittedCode = answerDto?.SubmittedCode;
						existingResponse.IsCorrect = isCorrect;
						existingResponse.ConsoleOutput = consoleOutput;
						existingResponse.ExecutionResultsJson = executionResultsJson;
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
							SubmittedCode = answerDto?.SubmittedCode,
							ConsoleOutput = consoleOutput,
							ExecutionResultsJson = executionResultsJson
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
					if (question.QuestionType == QuestionType.Coding)
					{
						totalTestCases += 1;
					}
				}

				if (isCorrect) correctAnswers++;

				// Concept/Subtopic Aggregation
				string concept = string.IsNullOrWhiteSpace(question.Concept) ? "General Theory" : question.Concept.Trim();
				if (!conceptScores.ContainsKey(concept))
				{
					conceptScores[concept] = (0, 0);
				}
				var current = conceptScores[concept];
				conceptScores[concept] = (current.Correct + (isCorrect ? 1 : 0), current.Total + 1);
			}

			// Applied Weighted Scoring Engine
			int mcqScore = totalMcqs > 0 ? (int)Math.Round(((double)correctMcqs / totalMcqs) * 100) : 0;
			int codingScore = totalTestCases > 0 ? (int)Math.Round(((double)totalPassedTestCases / totalTestCases) * 100) : 0;

			int compositeScore;
			if (totalMcqs > 0 && totalCodingQuestions > 0)
			{
				// 30% conceptual MCQ + 70% applied coding
				compositeScore = (int)Math.Round((mcqScore * 0.30) + (codingScore * 0.70));
			}
			else if (totalCodingQuestions > 0)
			{
				compositeScore = codingScore;
			}
			else if (totalMcqs > 0)
			{
				compositeScore = mcqScore;
			}
			else
			{
				compositeScore = totalQuestions > 0 ? (int)Math.Round(((double)correctAnswers / totalQuestions) * 100) : 0;
			}

			// Scaled Cut-Score Bar based on claimed level
			int passingScore = GetPassingThreshold(batch.AssignedSkill.ProficiencyLevel);
			bool passed = compositeScore >= passingScore;

			// Banded Verification Status
			string verificationStatus;
			string verificationMessage;
			ProficiencyLevel placedLevel = batch.AssignedSkill.ProficiencyLevel;

			if (compositeScore >= 85)
			{
				verificationStatus = "StronglyVerified";
				verificationMessage = $"Strongly verified proficiency at the {batch.AssignedSkill.ProficiencyLevel} level.";
			}
			else if (passed)
			{
				verificationStatus = "PartiallyVerified";
				verificationMessage = $"Verified baseline proficiency at the {batch.AssignedSkill.ProficiencyLevel} level with identified growth areas.";
			}
			else
			{
				verificationStatus = "Unverified";
				verificationMessage = $"Assessment score ({compositeScore}%) did not meet the {passingScore}% threshold for {batch.AssignedSkill.ProficiencyLevel}. Placement has been recalibrated.";
				
				// Recalibrate placement to 1 tier lower (bounded at Novice)
				placedLevel = batch.AssignedSkill.ProficiencyLevel switch
				{
					ProficiencyLevel.Expert => ProficiencyLevel.Proficient,
					ProficiencyLevel.Proficient => ProficiencyLevel.Intermediate,
					ProficiencyLevel.Intermediate => ProficiencyLevel.Begineer,
					ProficiencyLevel.Begineer => ProficiencyLevel.Novice,
					_ => ProficiencyLevel.Novice
				};
				batch.AssignedSkill.ProficiencyLevel = placedLevel;
			}

			// Mark AssignedSkill as baseline assessed
			batch.AssignedSkill.IsBaselineAssessed = true;
			batch.AssignedSkill.BaselineAssessedAt = DateTime.UtcNow;
			await _unitOfWork.AssignedSkills.UpdateAsync(batch.AssignedSkill);

			var result = new AssessmentResult
			{
				AssessmentBatchId = batch.Id,
				SkillId = batch.SkillId,
				TotalQuestions = totalQuestions,
				NoOfCorrectAnswers = correctAnswers,
				NoOfWrongAnswers = totalQuestions - correctAnswers,
				NoOfUnansweredQuestions = unansweredCount,
				Score = compositeScore,
				McqScore = mcqScore,
				CodingScore = codingScore,
				VerificationStatus = verificationStatus,
				PlacedProficiencyLevel = placedLevel.ToString(),
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

			if (verificationStatus == "StronglyVerified")
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
					result.PlacedProficiencyLevel = newLevel.ToString();
					await _unitOfWork.AssignedSkills.UpdateAsync(batch.AssignedSkill);
					targetBadgeLevel = newLevel.ToString();
				}
				else
				{
					batch.AssignedSkill.IsFullyMastered = true;
					await _unitOfWork.AssignedSkills.UpdateAsync(batch.AssignedSkill);
					targetBadgeLevel = "Master";
				}

				// Award Milestone Badges
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

			// Identify Gaps
			var skillGaps = new List<SkillGap>();
			foreach (var kvp in conceptScores)
			{
				string conceptName = kvp.Key;
				var stats = kvp.Value;
				int conceptScore = stats.Total > 0 ? (int)((double)stats.Correct / stats.Total * 100) : 0;

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

			// Clean up previous active gaps
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

			// Generate Tailored Improvement Plan
			var improvementPlan = await _aiService.GenerateImprovementPlanAsync(result, skillGaps);
			improvementPlan.AssessmentResultId = result.Id;
			await _unitOfWork.ImprovementPlans.AddAsync(improvementPlan);
			
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			// Award XP and log the activity for the learner/team member
			int xpEarned = 50 + (int)Math.Round(compositeScore / 2.0);
			if (levelUp) xpEarned += 25;
			if (badgeUnlocked) xpEarned += 30;

			var activityDescription = passed
				? $"Completed {batch.AssignedSkill.Name} assessment with {compositeScore}%."
				: $"Completed {batch.AssignedSkill.Name} assessment with {compositeScore}% and recalibrated placement.";

			await _activityLogService.AwardPointsAsync(
				request.UserId,
				request.UserRole,
				Domain.Enum.UserActivityType.AssessmentCompleted,
				activityDescription,
				xpEarned,
				"AssessmentResult",
				result.Id);

			var responseDto = new AssessmentResultDTO
			{
				Id = result.Id,
				SkillName = batch.AssignedSkill.Name,
				Score = result.Score,
				McqScore = mcqScore,
				CodingScore = codingScore,
				VerificationStatus = verificationStatus,
				PlacedProficiencyLevel = placedLevel.ToString(),
				VerificationMessage = verificationMessage,
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
				BadgeTitle = badgeTitle,
				XpEarned = xpEarned
			};

			return BaseResponse<AssessmentResultDTO>.SuccessResponse(responseDto, "Assessment submitted and graded successfully.");
		}

		private static int GetPassingThreshold(ProficiencyLevel level)
		{
			return level switch
			{
				ProficiencyLevel.Novice => 65,
				ProficiencyLevel.Begineer => 65,
				ProficiencyLevel.Intermediate => 75,
				ProficiencyLevel.Proficient => 75,
				ProficiencyLevel.Expert => 80,
				_ => 75
			};
		}
	}
}

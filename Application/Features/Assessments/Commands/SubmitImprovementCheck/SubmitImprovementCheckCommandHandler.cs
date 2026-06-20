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

namespace Application.Features.Assessments.Commands.SubmitImprovementCheck
{
	public class SubmitImprovementCheckCommandHandler : IRequestHandler<SubmitImprovementCheckCommand, BaseResponse<AssessmentResultDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICodeExecutionService _codeExecutionService;

		public SubmitImprovementCheckCommandHandler(IUnitOfWork unitOfWork, ICodeExecutionService codeExecutionService)
		{
			_unitOfWork = unitOfWork;
			_codeExecutionService = codeExecutionService;
		}

		public async Task<BaseResponse<AssessmentResultDTO>> Handle(SubmitImprovementCheckCommand request, CancellationToken cancellationToken)
		{
			// Load batch along with assessments and options
			var batch = await _unitOfWork.AssessmentBatches.GetBatchForGradingAsync(request.requestDto.AssessmentBatchId);
			if (batch == null)
			{
				throw new NotFoundException("Assessment Batch", request.requestDto.AssessmentBatchId);
			}

			if (batch.AssessmentStatus == AssessmentStatus.Completed)
			{
				throw new AssessmentAlreadyCompletedException();
			}

			if (batch.BatchType != "ImprovementCheck")
			{
				throw new BadRequestException("This batch is not an Improvement Check batch.");
			}

			// Timer enforcement (with 1-minute grace period)
			if (batch.StartedAt.HasValue && batch.TimeLimitMinutes.HasValue)
			{
				var deadline = batch.StartedAt.Value.AddMinutes(batch.TimeLimitMinutes.Value).AddMinutes(1);
				if (DateTime.UtcNow > deadline)
				{
					throw new BadRequestException("Assessment time limit exceeded. Your submission was not accepted in time.");
				}
			}

			// Parallel Execution for Coding Questions
			var codingTasks = new List<(Assessment Question, UserAnswerDTO Answer, Task<CodeExecutionResponseDTO> Task)>();
			foreach (var question in batch.Assessments.Where(q => q.QuestionType == QuestionType.Coding))
			{
				var answerDto = request.requestDto.UserAnswers.FirstOrDefault(a => a.AssessmentQuestionId == question.Id);
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
			int totalQuestions = batch.Assessments.Count;
			int unansweredCount = 0;
			var userResponses = new List<UserResponse>();

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
						IsCorrect = isCorrect,
						SubmittedCode = answerDto?.SubmittedCode
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
			}

			int score = totalQuestions > 0 ? (int)((double)correctAnswers / totalQuestions * 100) : 0;
			bool passed = score >= 100; // Require 100% correct answers (3 out of 3) to clear a specific gap

			// Save AssessmentResult
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
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			// Load the active SkillGap for this concept
			var gaps = await _unitOfWork.SkillGaps.FindAsync(
				g => g.SkillId == batch.AssignedSkill.Id &&
				     g.Concept == batch.ConceptFocus &&
				     g.Status == "Active" &&
				     (request.UserRole == Roles.Learner.ToString() ? g.LearnerId == request.UserId : g.TeamMemberId == request.UserId)
			);
			var gap = gaps.FirstOrDefault();

			if (gap != null)
			{
				if (passed)
				{
					// Resolve gap
					gap.Status = "Resolved";
					gap.Score = score;
					await _unitOfWork.SkillGaps.UpdateAsync(gap);
				}
				else
				{
					// If they failed the check, reset completed study tasks back to "Pending" so they must study again
					var plans = await _unitOfWork.ImprovementPlans.FindAsync(
						p => p.AssessmentResultId == gap.AssessmentResultId,
						p => p.Tasks
					);
					var plan = plans.FirstOrDefault();
					if (plan != null)
					{
						var conceptTasks = plan.Tasks.Where(t => t.Concept.Equals(batch.ConceptFocus, StringComparison.OrdinalIgnoreCase)).ToList();
						foreach (var task in conceptTasks)
						{
							task.Status = "Pending";
							task.CompletedAt = null;
							await _unitOfWork.ImprovementTasks.UpdateAsync(task);
						}
					}
				}
				await _unitOfWork.SaveChangesAsync(cancellationToken);
			}

			var responseDto = new AssessmentResultDTO
			{
				Id = result.Id,
				SkillName = batch.AssignedSkill.Name + " - " + batch.ConceptFocus,
				Score = result.Score,
				NoOfCorrectAnswers = result.NoOfCorrectAnswers,
				NoOfWrongAnswers = result.NoOfWrongAnswers,
				TotalQuestions = result.TotalQuestions,
				ProficiencyLevel = result.ProficiencyLevel.ToString(),
				DateCompleted = result.DateCreated,
				Passed = passed,
				PassingScore = 100 // 100% is the passing mark for micro-assessments
			};

			return BaseResponse<AssessmentResultDTO>.SuccessResponse(responseDto, "Improvement check submitted and graded successfully.");
		}
	}
}

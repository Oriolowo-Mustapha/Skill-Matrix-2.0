using Application.DTOs;
using Application.DTOs.Assessments;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Assessments.Queries.GetAssessmentDetail
{
	public class GetAssessmentDetailQueryHandler : IRequestHandler<GetAssessmentDetailQuery, BaseResponse<AssessmentDetailDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetAssessmentDetailQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<AssessmentDetailDTO>> Handle(GetAssessmentDetailQuery request, CancellationToken cancellationToken)
		{
			var result = await _unitOfWork.AssessmentResults.GetByIdAsync(request.ResultId);
			if (result == null)
			{
				throw new NotFoundException("Assessment Result", request.ResultId);
			}

			// Validate ownership unless manager/admin
			bool isOwner = result.LearnerID == request.UserId || result.TeamMemberID == request.UserId;
			bool isManager = request.UserRole == Roles.Manager.ToString() || request.UserRole == Roles.Admin.ToString();
			if (!isOwner && !isManager)
			{
				throw new UnauthorizedAccessException("You are not authorized to view this assessment breakdown.");
			}

			string skillName = "Technical Assessment";
			if (result.SkillId != Guid.Empty)
			{
				var assignedSkill = await _unitOfWork.AssignedSkills.GetByIdAsync(result.SkillId);
				if (assignedSkill != null)
				{
					skillName = assignedSkill.Name;
				}
			}

			var batch = await _unitOfWork.AssessmentBatches.GetBatchForGradingAsync(result.AssessmentBatchId);
			var userResponses = (await _unitOfWork.UserResponses.FindAsync(ur => ur.AssessmentBatchId == result.AssessmentBatchId)).ToList();

			var questionsReview = new List<QuestionReviewDTO>();
			var questionsList = batch?.Assessments ?? new List<Assessment>();

			foreach (var q in questionsList)
			{
				var userResp = userResponses.FirstOrDefault(ur => ur.AssessmentQuestionId == q.Id);
				bool isAnswered = userResp != null && (userResp.SelectedOptionId.HasValue || !string.IsNullOrWhiteSpace(userResp.SubmittedCode));
				bool isCorrect = userResp?.IsCorrect ?? false;
				bool isFlagged = userResp?.IsFlagged ?? false;

				var reviewItem = new QuestionReviewDTO
				{
					QuestionId = q.Id,
					QuestionText = q.Questions,
					QuestionType = q.QuestionType == QuestionType.Coding ? "Coding" : "MultipleChoice",
					Concept = string.IsNullOrWhiteSpace(q.Concept) ? "Core Concept" : q.Concept,
					IsCorrect = isCorrect,
					IsAnswered = isAnswered,
					IsFlagged = isFlagged
				};

				if (q.QuestionType == QuestionType.Coding)
				{
					var testResults = new List<TestCaseReviewResultDTO>();
					if (!string.IsNullOrWhiteSpace(userResp?.ExecutionResultsJson))
					{
						try
						{
							testResults = JsonSerializer.Deserialize<List<TestCaseReviewResultDTO>>(userResp.ExecutionResultsJson, new JsonSerializerOptions
							{
								PropertyNameCaseInsensitive = true
							}) ?? new List<TestCaseReviewResultDTO>();
						}
						catch
						{
							// Fallback if parsing fails
						}
					}

					reviewItem.CodingDetail = new CodingReviewDetailDTO
					{
						Language = string.IsNullOrWhiteSpace(q.CorrectAnswer) ? "typescript" : q.CorrectAnswer.Trim(),
						SubmittedCode = userResp?.SubmittedCode ?? q.CodeTemplate,
						SampleInput = q.SampleInput,
						ExpectedOutput = q.ExpectedOutput,
						ConsoleOutput = userResp?.ConsoleOutput,
						FunctionName = q.FunctionName ?? "Solve",
						TestResults = testResults
					};
				}
				else
				{
					var optionsList = (q.AssessmentOptions ?? new List<AssessmentOptions>()).Select(opt => new OptionReviewDTO
					{
						Id = opt.Id,
						OptionText = opt.OptionText,
						IsSelected = userResp?.SelectedOptionId == opt.Id,
						IsCorrectOption = string.Equals(opt.OptionText?.Trim(), q.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase)
					}).ToList();

					var selectedOpt = optionsList.FirstOrDefault(o => o.IsSelected);

					reviewItem.McqDetail = new MCQReviewDetailDTO
					{
						SelectedOptionId = userResp?.SelectedOptionId,
						SelectedOptionText = selectedOpt?.OptionText,
						CorrectAnswerText = q.CorrectAnswer,
						Options = optionsList
					};
				}

				questionsReview.Add(reviewItem);
			}

			var detailDto = new AssessmentDetailDTO
			{
				ResultId = result.Id,
				SkillName = skillName,
				ProficiencyLevel = result.ProficiencyLevel.ToString(),
				Score = result.Score,
				McqScore = result.McqScore,
				CodingScore = result.CodingScore,
				VerificationStatus = result.VerificationStatus ?? "PartiallyVerified",
				PlacedProficiencyLevel = result.PlacedProficiencyLevel ?? result.ProficiencyLevel.ToString(),
				Passed = result.Score >= (result.ProficiencyLevel == ProficiencyLevel.Expert ? 80 : result.ProficiencyLevel >= ProficiencyLevel.Intermediate ? 75 : 65),
				TotalQuestions = result.TotalQuestions,
				NoOfCorrectAnswers = result.NoOfCorrectAnswers,
				NoOfWrongAnswers = result.NoOfWrongAnswers,
				NoOfUnansweredQuestions = result.NoOfUnansweredQuestions,
				DateCompleted = result.DateCreated,
				ImprovementPlanId = result.ImprovementPlanId,
				Questions = questionsReview
			};

			return BaseResponse<AssessmentDetailDTO>.SuccessResponse(detailDto, "Assessment details retrieved successfully.");
		}
	}
}

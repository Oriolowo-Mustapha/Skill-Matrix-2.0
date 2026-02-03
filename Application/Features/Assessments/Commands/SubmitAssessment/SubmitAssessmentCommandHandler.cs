using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;
using MediatR;

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

			int correctAnswers = 0;
			int totalQuestions = batch.Assessments.Count;
			var userResponses = new List<UserResponse>();

			foreach (var answerDto in request.requestDto.UserAnswers)
			{
				var question = batch.Assessments.FirstOrDefault(q => q.Id == answerDto.AssessmentQuestionId);

				if (question == null) continue;
				var response = new UserResponse
				{
					AssessmentBatchId = batch.Id,
					AssessmentQuestionId = question.Id,
					SelectedOptionId = answerDto.SelectedOptionId,
					Timestamp = DateTime.UtcNow
				};

				if (request.UserRole == Roles.Learner.ToString())
					response.LearnerId = request.UserId;
				else
					response.TeamMemberId = request.UserId;

				var selectedOption = question.AssessmentOptions.FirstOrDefault(o => o.Id == answerDto.SelectedOptionId);

				bool isCorrect = false;
				if (selectedOption != null && selectedOption.OptionText == question.CorrectAnswer)
				{
					isCorrect = true;
					correctAnswers++;
				}

				response.IsCorrect = isCorrect;
				userResponses.Add(response);
			}

			int score = (int)((double)correctAnswers / totalQuestions * 100);

			var result = new AssessmentResult
			{
				AssessmentBatchId = batch.Id,
				SkillId = batch.SkillId,
				TotalQuestions = totalQuestions,
				NoOfCorrectAnswers = correctAnswers,
				NoOfWrongAnswers = totalQuestions - correctAnswers,
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

			// Generate Improvement Plan
			var improvementPlan = await _aiService.GenerateImprovementPlanAsync(result);
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
				DateCompleted = result.DateCreated
			};
		}
	}
}

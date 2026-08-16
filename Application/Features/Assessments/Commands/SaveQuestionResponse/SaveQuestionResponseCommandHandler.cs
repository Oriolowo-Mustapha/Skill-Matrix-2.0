using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enum;
using MediatR;

namespace Application.Features.Assessments.Commands.SaveQuestionResponse
{
	public class SaveQuestionResponseCommandHandler : IRequestHandler<SaveQuestionResponseCommand, BaseResponse<SaveQuestionResponseResultDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public SaveQuestionResponseCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<SaveQuestionResponseResultDTO>> Handle(SaveQuestionResponseCommand request, CancellationToken cancellationToken)
		{
			var batch = await _unitOfWork.AssessmentBatches.GetBatchWithQuestionsAsync(request.BatchId);
			if (batch == null)
			{
				throw new NotFoundException("Assessment Batch", request.BatchId);
			}

			// Validate ownership
			if (request.UserRole == Roles.Learner.ToString() && batch.LearnerID != request.UserId)
			{
				throw new UnauthorizedException("You are not authorized to modify this assessment batch.");
			}
			if ((request.UserRole == Roles.Team_Members.ToString() || request.UserRole == "TeamMember") && batch.TeamMemberID != request.UserId)
			{
				throw new UnauthorizedException("You are not authorized to modify this assessment batch.");
			}

			// Validate status
			if (batch.AssessmentStatus == AssessmentStatus.Completed)
			{
				return BaseResponse<SaveQuestionResponseResultDTO>.FailureResponse("Assessment is already completed.");
			}

			var now = DateTime.UtcNow;
			var startedAt = batch.StartedAt ?? batch.DateCreated;
			int limitMins = batch.TimeLimitMinutes ?? 30;
			var expiresAt = batch.ExpiresAt ?? startedAt.AddMinutes(limitMins);
			int secondsRemaining = Math.Max(0, (int)(expiresAt - now).TotalSeconds);

			// Server owns the clock: reject writes past deadline (+ 2 min grace period)
			if (now > expiresAt.AddMinutes(2))
			{
				return BaseResponse<SaveQuestionResponseResultDTO>.FailureResponse("Assessment time limit exceeded.");
			}

			// Update active question index if supplied
			if (request.Dto.CurrentQuestionIndex.HasValue && request.Dto.CurrentQuestionIndex.Value >= 0)
			{
				batch.LastActiveQuestionIndex = request.Dto.CurrentQuestionIndex.Value;
				await _unitOfWork.AssessmentBatches.UpdateAsync(batch);
			}

			// Sanitize SelectedOptionId (must be > 0 or null)
			int? safeOptionId = (request.Dto.SelectedOptionId.HasValue && request.Dto.SelectedOptionId.Value > 0)
				? request.Dto.SelectedOptionId.Value
				: null;

			// Idempotent UPSERT on UserResponse for (AssessmentBatchId, AssessmentQuestionId)
			var existingList = await _unitOfWork.UserResponses.FindAsync(
				ur => ur.AssessmentBatchId == request.BatchId && ur.AssessmentQuestionId == request.QuestionId
			);
			var existing = existingList.FirstOrDefault();

			if (existing != null)
			{
				existing.SelectedOptionId = safeOptionId;
				existing.SubmittedCode = request.Dto.SubmittedCode;
				existing.IsFlagged = request.Dto.IsFlagged;
				existing.UpdatedAt = now;
				await _unitOfWork.UserResponses.UpdateAsync(existing);
			}
			else
			{
				var newResponse = new UserResponse
				{
					AssessmentBatchId = batch.Id,
					AssessmentQuestionId = request.QuestionId,
					SelectedOptionId = safeOptionId,
					SubmittedCode = request.Dto.SubmittedCode,
					IsFlagged = request.Dto.IsFlagged,
					Timestamp = now,
					UpdatedAt = now
				};

				if (request.UserRole == Roles.Learner.ToString())
					newResponse.LearnerId = request.UserId;
				else
					newResponse.TeamMemberId = request.UserId;

				await _unitOfWork.UserResponses.AddAsync(newResponse);
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return BaseResponse<SaveQuestionResponseResultDTO>.SuccessResponse(
				new SaveQuestionResponseResultDTO
				{
					Success = true,
					ServerUpdatedAt = now,
					SecondsRemaining = secondsRemaining,
					IsExpired = false,
					Message = "Saved"
				},
				"Response saved successfully."
			);
		}
	}
}

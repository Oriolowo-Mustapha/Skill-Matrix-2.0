using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enum;
using MediatR;

namespace Application.Features.Assessments.Queries.GetAssessmentAttemptState
{
	public class GetAssessmentAttemptStateQueryHandler : IRequestHandler<GetAssessmentAttemptStateQuery, BaseResponse<AssessmentAttemptStateDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetAssessmentAttemptStateQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<AssessmentAttemptStateDTO>> Handle(GetAssessmentAttemptStateQuery request, CancellationToken cancellationToken)
		{
			var batch = await _unitOfWork.AssessmentBatches.GetBatchForGradingAsync(request.BatchId);
			if (batch == null)
			{
				throw new NotFoundException("Assessment Batch", request.BatchId);
			}

			// Validate ownership
			if (request.UserRole == Roles.Learner.ToString() && batch.LearnerID != request.UserId)
			{
				throw new UnauthorizedException("You are not authorized to view this assessment batch.");
			}
			if ((request.UserRole == Roles.Team_Members.ToString() || request.UserRole == "TeamMember") && batch.TeamMemberID != request.UserId)
			{
				throw new UnauthorizedException("You are not authorized to view this assessment batch.");
			}

			var now = DateTime.UtcNow;
			var startedAt = batch.StartedAt ?? batch.DateCreated;
			int limitMins = batch.TimeLimitMinutes ?? 30;
			var expiresAt = batch.ExpiresAt ?? startedAt.AddMinutes(limitMins);
			int secondsRemaining = Math.Max(0, (int)(expiresAt - now).TotalSeconds);

			// Fetch existing saved user responses for this batch
			var existingResponses = await _unitOfWork.UserResponses.FindAsync(ur => ur.AssessmentBatchId == batch.Id);

			var savedResponseDtos = existingResponses.Select(r => new SavedQuestionResponseDTO
			{
				QuestionId = r.AssessmentQuestionId,
				SelectedOptionId = r.SelectedOptionId,
				SubmittedCode = r.SubmittedCode,
				IsFlagged = r.IsFlagged,
				UpdatedAt = r.UpdatedAt
			}).ToList();

			var questionDtos = batch.Assessments.Select(q => q.ToDTO()).ToList();

			var stateDto = new AssessmentAttemptStateDTO
			{
				AssessmentBatchId = batch.Id,
				AssignedSkillId = batch.SkillId,
				SkillName = batch.AssignedSkill?.Name ?? "Skill",
				ProficiencyLevel = batch.AssignedSkill?.ProficiencyLevel.ToString() ?? "Novice",
				BatchType = batch.BatchType,
				Status = batch.AssessmentStatus.ToString(),
				StartedAt = startedAt,
				ExpiresAt = expiresAt,
				ServerTimeUtc = now,
				SecondsRemaining = secondsRemaining,
				LastActiveQuestionIndex = batch.LastActiveQuestionIndex,
				TimeLimitMinutes = limitMins,
				Questions = questionDtos,
				SavedResponses = savedResponseDtos
			};

			return BaseResponse<AssessmentAttemptStateDTO>.SuccessResponse(stateDto, "Assessment attempt state retrieved successfully.");
		}
	}
}

using System;
using Application.DTOs;
using MediatR;

namespace Application.Features.Assessments.Queries.GetAssessmentAttemptState
{
	public class GetAssessmentAttemptStateQuery : IRequest<BaseResponse<AssessmentAttemptStateDTO>>
	{
		public int BatchId { get; set; }
		public Guid UserId { get; set; }
		public string UserRole { get; set; } = string.Empty;

		public GetAssessmentAttemptStateQuery(int batchId, Guid userId, string userRole)
		{
			BatchId = batchId;
			UserId = userId;
			UserRole = userRole;
		}
	}
}

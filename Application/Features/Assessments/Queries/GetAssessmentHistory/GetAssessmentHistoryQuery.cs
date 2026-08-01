using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Assessments.Queries.GetAssessmentHistory
{
	public record GetAssessmentHistoryQuery(Guid UserId, string UserRole) : IRequest<BaseResponse<List<AssessmentResultDTO>>>;
}

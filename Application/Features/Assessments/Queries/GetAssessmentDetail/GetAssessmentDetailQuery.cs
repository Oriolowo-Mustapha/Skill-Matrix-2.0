using Application.DTOs;
using Application.DTOs.Assessments;
using MediatR;
using System;

namespace Application.Features.Assessments.Queries.GetAssessmentDetail
{
	public record GetAssessmentDetailQuery(Guid ResultId, Guid UserId, string UserRole) : IRequest<BaseResponse<AssessmentDetailDTO>>;
}

using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Assessments.Queries.GetAssessmentResult
{
	public record GetAssessmentResultQuery(Guid ResultId, Guid UserId) : IRequest<AssessmentResultDTO>;
}

using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Assessments.Commands.SubmitImprovementCheck
{
	public record SubmitImprovementCheckCommand(SubmitAssessmentRequestDTO requestDto, Guid UserId, string UserRole) : IRequest<AssessmentResultDTO>;
}

using Application.DTOs;
using MediatR;

namespace Application.Features.Assessments.Commands.SubmitAssessment
{
	public record SubmitAssessmentCommand(SubmitAssessmentRequestDTO requestDto, Guid UserId, string UserRole) : IRequest<AssessmentResultDTO>;
}

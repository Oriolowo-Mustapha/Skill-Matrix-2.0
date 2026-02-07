using Application.DTOs;
using MediatR;

namespace Application.Features.Assessments.Commands.StartAssessment
{
	public record StartAssessmentCommand(AssesmentDTO Dto, Guid UserId, string UserRole) : IRequest<StartAssessmentResponseDTO>;
}

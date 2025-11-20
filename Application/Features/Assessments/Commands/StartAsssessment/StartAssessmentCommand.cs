using Application.DTOs;
using MediatR;

namespace Application.Features.Assessments.Commands.StartAsssessment
{
	public record StartAssessmentCommand(AssesmentDTO AssesmentDTO, Guid UserId) : IRequest<StartAssesmentResponseDTO>;
}

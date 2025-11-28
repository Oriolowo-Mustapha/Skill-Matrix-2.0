using Application.DTOs;
using MediatR;

namespace Application.Features.Assessments.Commands.StartAsssessment
{
	public record StartAssessmentCommand(AssesmentDTO Dto, Guid UserId, string UserRole) : IRequest<StartAssesmentResponseDTO>;
}

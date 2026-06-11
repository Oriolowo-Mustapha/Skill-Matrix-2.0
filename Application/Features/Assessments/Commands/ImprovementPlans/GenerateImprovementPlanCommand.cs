using Application.DTOs;
using MediatR;

namespace Application.Features.Assessments.Commands.ImprovementPlans
{
	public record GenerateImprovementPlanCommand(Guid AssessmentResultId, Guid UserId, string UserRole) : IRequest<BaseResponse<ImprovementPlanDTO>>;

}

using Application.DTOs;
using MediatR;

namespace Application.Features.ImprovementPlans.Commands.GenerateAiImprovementPlan
{
	public class GenerateAiImprovementPlanCommand : IRequest<BaseResponse<AIImprovementPlanResponseDTO>>
	{
		public Guid TeamMemberId { get; set; }
		public Guid TargetCareerPathId { get; set; }
	}
}

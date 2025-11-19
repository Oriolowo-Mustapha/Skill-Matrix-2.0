using Application.DTOs;

namespace Application.Interfaces.Service
{
	public interface IImprovementPlanService
	{
		Task<ImprovementPlanDTO> GenerateImprovementPlanAsync(Guid assesmentResultId, Guid userId);
		Task<ImprovementPlanDTO> GetImprovementPlanAsync(Guid PlanId, Guid userId);

	}
}

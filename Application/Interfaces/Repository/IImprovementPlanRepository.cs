using Domain.Entities;

namespace Application.Interfaces.Repository
{
	public interface IImprovementPlanRepository : IGenericRepository<ImprovementPlan>
	{
		Task<ImprovementPlan?> GetPlanWIthResoursesAsync(Guid planId);
		Task<ImprovementPlan?> GetByAssessmentResultIdAsync(Guid assessmentResultId);
	}
}

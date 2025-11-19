using Domain.Entities;

namespace Application.Interfaces.Service
{
	public interface IAiService
	{
		Task<List<Assessment>> GeneratAssessmentQuestionsAsync(string skillName, int count, string proficencyLevel);
		Task<ImprovementPlan> GetImprovementPlanAsync(AssessmentResult result);
	}
}

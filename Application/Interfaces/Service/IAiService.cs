using Domain.Entities;

namespace Application.Interfaces.Service
{
	public interface IAiService
	{
		Task<IEnumerable<Assessment>> GenerateAssessmentQuestionsAsync(string skillName, string proficencyLevel, int count);
		Task<ImprovementPlan> GenerateImprovementPlanAsync(AssessmentResult result);
	}
}

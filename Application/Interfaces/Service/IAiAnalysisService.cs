using Application.DTOs;

namespace Application.Interfaces.Service
{
	public interface IAiAnalysisService
	{
		Task<string> GenerateImprovementPlanAsync(List<AssessmentResultDTO> assessmentResults, CareerPathDTO targetCareerPath);
	}
}

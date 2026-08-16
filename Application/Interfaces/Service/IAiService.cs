using Domain.Entities;

namespace Application.Interfaces.Service
{
	public interface IAiService
	{
		Task<IEnumerable<Assessment>> GenerateAssessmentQuestionsAsync(string skillName, string proficencyLevel, int mcqCount, int codingCount, bool requiresCoding);
		Task<Application.DTOs.Ai.AssessmentPackageResult> GenerateAssessmentPackageAsync(string skillName, string proficencyLevel, int mcqCount, int codingCount, bool requiresCoding);
		Task<ImprovementPlan> GenerateImprovementPlanAsync(AssessmentResult result, List<SkillGap> gaps);
		Task<ImprovementPlan> GenerateStarterPlanAsync(string skillName, string category);
		Task<bool> ClassifySkillRequiresCodingAsync(string skillName);
		Task<IEnumerable<Assessment>> GenerateTargetedQuestionsAsync(string skillName, string proficencyLevel, string concept, int count, bool requiresCoding);
		Task<List<Application.DTOs.Ai.GeneratedTrackSkillDto>> GenerateSkillsForTrackAsync(string careerPathTitle, string trackName);
		Task<List<Application.DTOs.Ai.AiCatalogPathDto>> GenerateGlobalCatalogAsync(List<string> existingSkillNames);
		Task<List<Application.DTOs.Ai.AiSkillCatalogItemDto>> GenerateSkillCatalogAsync();
	}
}

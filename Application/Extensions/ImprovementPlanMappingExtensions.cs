using Application.DTOs;
using Domain.Entities;

namespace Application.Extensions
{
	public static class ImprovementPlanMappingExtensions
	{
		public static AssessmentResultDTO ToDTO(this AssessmentResult assessmentResult, string skillName)
		{
			return new AssessmentResultDTO
			{
				Id = assessmentResult.Id,
				SkillName = skillName,
				Score = assessmentResult.Score,
				NoOfCorrectAnswers = assessmentResult.NoOfCorrectAnswers,
				NoOfWrongAnswers = assessmentResult.NoOfWrongAnswers,
				TotalQuestions = assessmentResult.TotalQuestions,
				ProficiencyLevel = assessmentResult.ProficiencyLevel.ToString(),
				DateCompleted = assessmentResult.DateCreated
			};
		}

		public static ImprovementPlanDTO ToDto(this ImprovementPlan plan)
		{
			return new ImprovementPlanDTO
			{
				Id = plan.Id,
				GeneratedSummary = plan.GeneratedSummary,
				FocusAreas = plan.FocusArea,
				DateGenerated = plan.DateGenerated,
				RecommendedResources = plan.RecommendedResources.Select(p => p.ToDto()).ToList()
			};
		}
		public static RecommendedResourceDTO ToDto(this RecommendedResource resource)
		{
			return new RecommendedResourceDTO
			{
				Id = resource.Id,
				Title = resource.Title,
				Url = resource.Url,
				Description = resource.Description,
				ResourseType = resource.ResourseType.ToString()
			};
		}
	}
}

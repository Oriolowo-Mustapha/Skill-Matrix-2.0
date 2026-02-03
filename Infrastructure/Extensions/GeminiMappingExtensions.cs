using Domain.Entities;
using Domain.Enum;
using Infrastructure.DTOs;
using System.Linq;

namespace Infrastructure.Extensions
{
	public static class GeminiMappingExtensions
	{
		public static Assessment ToEntity(this GeminiQuestionDto dto)
		{
			return new Assessment
			{
				Questions = dto.QuestionText,
				CorrectAnswer = dto.CorrectAnswer,
				AssessmentOptions = dto.Options.Select(o => new AssessmentOptions { OptionText = o }).ToList()
			};
		}

		public static ImprovementPlan ToEntity(this GeminiPlanDto dto)
		{
			return new ImprovementPlan
			{
				GeneratedSummary = dto.Summary,
				FocusArea = dto.FocusAreas,
				RecommendedResources = dto.Resources.Select(r => r.ToEntity()).ToList()
			};
		}

		public static RecommendedResource ToEntity(this GeminiResourceDto dto)
		{
			return new RecommendedResource
			{
				Title = dto.Title,
				Url = dto.Url,
				Description = "AI Recommended",
				ResourseType = Enum.TryParse<ResourseType>(dto.Type, true, out var t) ? t : ResourseType.Article
			};
		}
	}
}

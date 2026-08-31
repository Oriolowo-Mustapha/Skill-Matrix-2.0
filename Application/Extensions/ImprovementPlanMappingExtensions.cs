using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

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

		public static ImprovementPlanDTO ToDto(this ImprovementPlan plan, string skillName = "")
		{
			return new ImprovementPlanDTO
			{
				Id = plan.Id,
				AssessmentResultId = plan.AssessmentResultId,
				GeneratedSummary = plan.GeneratedSummary,
				FocusAreas = plan.FocusArea,
				FocusArea = plan.FocusArea,
				SkillName = !string.IsNullOrWhiteSpace(skillName) ? skillName : (plan.AssignedSkill?.Name ?? ""),
				DateGenerated = plan.DateGenerated,
				IsStarterPlan = plan.IsStarterPlan,
				IsAiGenerated = plan.IsAiGenerated,
				RecommendedResources = (plan.RecommendedResources ?? new List<RecommendedResource>()).Select(p => p.ToDto()).ToList(),
				Tasks = (plan.Tasks ?? new List<ImprovementTask>()).Select(t => new ImprovementTaskDTO
				{
					Id = t.Id,
					Title = !string.IsNullOrWhiteSpace(t.Concept) ? t.Concept : t.Description,
					Description = t.Description,
					Concept = t.Concept,
					Status = t.Status,
					IsCompleted = string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase)
				}).ToList()
			};
		}

		public static ImprovementPlanDTO ToDTO(this ImprovementPlan plan, string skillName = "") => plan.ToDto(skillName);

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

		public static List<ImprovementPlanDTO> ToImprovementPlanDTOList(this IEnumerable<ImprovementPlan> plans)
		{
			return plans.Select(p => p.ToDto(p.AssignedSkill?.Name ?? "")).ToList();
		}
	}
}

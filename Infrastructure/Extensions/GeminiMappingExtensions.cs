using Domain.Entities;
using Domain.Enum;
using Infrastructure.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Extensions
{
	public static class GeminiMappingExtensions
	{
		public static Assessment ToEntity(this GeminiQuestionDto dto)
		{
			var isCoding = dto.QuestionType.Equals("Coding", StringComparison.OrdinalIgnoreCase);
			string? testCasesJson = null;
			if (dto.TestCases != null && dto.TestCases.Any())
			{
				testCasesJson = System.Text.Json.JsonSerializer.Serialize(dto.TestCases);
			}

			return new Assessment
			{
				Questions = dto.QuestionText,
				CorrectAnswer = dto.CorrectAnswer,
				QuestionType = isCoding ? QuestionType.Coding : QuestionType.MultipleChoice,
				ExpectedOutput = dto.ExpectedOutput ?? dto.TestCases?.FirstOrDefault(tc => !tc.IsHidden)?.ExpectedOutput,
				SampleInput = dto.SampleInput ?? dto.TestCases?.FirstOrDefault(tc => !tc.IsHidden)?.Input,
				CodeTemplate = dto.CodeTemplate,
				FunctionName = dto.FunctionName ?? "Solve",
				TestCases = testCasesJson,
				Concept = dto.Concept,
				AssessmentOptions = isCoding ? new List<AssessmentOptions>() : dto.Options.Select(o => new AssessmentOptions { OptionText = o }).ToList()
			};
		}

		public static ImprovementPlan ToEntity(this GeminiPlanDto dto)
		{
			var resources = dto.Resources.Select(r => r.ToEntity()).ToList();
			var tasks = dto.Tasks.Select(t =>
			{
				var matchingResource = resources.FirstOrDefault(r => r.Title.Equals(t.ResourceTitle, StringComparison.OrdinalIgnoreCase));
				return new ImprovementTask
				{
					Concept = t.Concept,
					Description = t.Description,
					Status = "Pending",
					RecommendedResource = matchingResource
				};
			}).ToList();

			return new ImprovementPlan
			{
				GeneratedSummary = dto.Summary,
				FocusArea = dto.FocusAreas,
				RecommendedResources = resources,
				Tasks = tasks
			};
		}

		public static RecommendedResource ToEntity(this GeminiResourceDto dto)
		{
			return new RecommendedResource
			{
				Title = dto.Title,
				Url = dto.Url,
				Description = "AI Recommended",
				Concept = dto.Concept,
				ResourseType = System.Enum.TryParse<ResourseType>(dto.Type, true, out var t) ? t : ResourseType.Article
			};
		}
	}
}

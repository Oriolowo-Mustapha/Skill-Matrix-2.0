using System;
using System.Collections.Generic;

namespace Application.DTOs
{
	public record ImprovementPlanDTO
	{
		public Guid Id { get; set; }
		public Guid? AssessmentResultId { get; set; }
		public string GeneratedSummary { get; set; } = string.Empty;
		public string FocusAreas { get; set; } = string.Empty;
		public string FocusArea { get; set; } = string.Empty;
		public string SkillName { get; set; } = string.Empty;
		public DateTime DateGenerated { get; set; }
		public bool IsStarterPlan { get; set; }
		public bool IsAiGenerated { get; set; }
		public List<RecommendedResourceDTO> RecommendedResources { get; set; } = new List<RecommendedResourceDTO>();
		public List<ImprovementTaskDTO> Tasks { get; set; } = new List<ImprovementTaskDTO>();
	}

	public record RecommendedResourceDTO
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string ResourseType { get; set; } = string.Empty;
	}

	public record ImprovementTaskDTO
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Concept { get; set; } = string.Empty;
		public string Status { get; set; } = "Pending";
		public bool IsCompleted { get; set; }
	}
}

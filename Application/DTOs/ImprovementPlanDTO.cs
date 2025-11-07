namespace Application.DTOs
{
	public record ImprovementPlanDTO
	{
		public Guid Id { get; set; }
		public Guid AssessmentResultId { get; set; }
		public string GeneratedSummary { get; set; } = string.Empty;
		public string FocusAreas { get; set; } = string.Empty;
		public DateTime DateGenerated { get; set; }
		public List<RecommendedResourceDTO> RecommendedResources { get; set; }
	}

	public record RecommendedResourceDTO
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		public string Description { get; set; }
		public string ResourseType { get; set; }
	}
}

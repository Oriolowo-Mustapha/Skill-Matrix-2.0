namespace Application.DTOs
{
	public class AIRecommendationDTO
	{
		public string CourseTitle { get; set; } = string.Empty;
		public string CourseUrl { get; set; } = string.Empty;
		public string ExpectedOutcome { get; set; } = string.Empty;
	}

	public class AIImprovementPlanResponseDTO
	{
		public string OverallSummary { get; set; } = string.Empty;
		public List<AIRecommendationDTO> RecommendedCourses { get; set; } = new();
		public List<string> FocusAreas { get; set; } = new();
	}
}

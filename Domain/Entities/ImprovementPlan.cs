namespace Domain.Entities
{
	public class ImprovementPlan : BaseEntity
	{
		public string GeneratedSummary { get; set; } = string.Empty;
		public string FocusArea { get; set; } = string.Empty;
		public DateTime DateGenerated { get; set; } = DateTime.UtcNow;
		public Guid AssessmentResultId { get; set; }
		public AssessmentResult AssessmentResult { get; set; } = null!;
		public List<RecommendedResource> RecommendedResources = new List<RecommendedResource>();
	}
}

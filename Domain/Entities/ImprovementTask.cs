using System;

namespace Domain.Entities
{
	public class ImprovementTask : BaseEntity
	{
		public Guid ImprovementPlanId { get; set; }
		public ImprovementPlan ImprovementPlan { get; set; } = null!;
		public string Concept { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Status { get; set; } = "Pending"; // "Pending", "Completed"
		public DateTime? CompletedAt { get; set; }
		public Guid? RecommendedResourceId { get; set; }
		public RecommendedResource? RecommendedResource { get; set; }
	}
}

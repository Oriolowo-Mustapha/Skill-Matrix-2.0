using Domain.Enum;

namespace Domain.Entities
{
	public class RecommendedResource : BaseEntity
	{
		public string Title { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public ResourseType ResourseType { get; set; }
		public Guid ImprovementPlanId { get; set; }
		public ImprovementPlan ImprovementPlan { get; set; } = null!;
	}
}

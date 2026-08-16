namespace Domain.Entities
{
	public class ImprovementPlan : BaseEntity
	{
		public string GeneratedSummary { get; set; } = string.Empty;
		public string FocusArea { get; set; } = string.Empty;
		public DateTime DateGenerated { get; set; } = DateTime.UtcNow;
		public Guid? AssessmentResultId { get; set; }
		public AssessmentResult? AssessmentResult { get; set; }
		public Guid? AssignedSkillId { get; set; }
		public AssignedSkill? AssignedSkill { get; set; }
		public List<RecommendedResource> RecommendedResources { get; set; } = new List<RecommendedResource>();
		public List<ImprovementTask> Tasks { get; set; } = new List<ImprovementTask>();
		public bool IsAiGenerated { get; set; } = false;
		public bool IsStarterPlan { get; set; } = false;
	}
}

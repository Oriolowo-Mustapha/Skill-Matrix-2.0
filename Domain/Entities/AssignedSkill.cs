namespace Domain.Entities
{
	public class AssignedSkill : BaseEntity
	{
		public string Name { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public Learner? Learner { get; set; }
		public Guid? LearnerId { get; set; }
		public TeamMember? TeamMember { get; set; }
		public Guid? TeamMemberId { get; set; }
		public Skill Skill { get; set; } = null!;
		public Guid SkillId { get; set; }
		public List<AssessmentBatch> AssessmentBatches { get; set; } = new List<AssessmentBatch>();
		public List<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();
		public List<AssignedBadge> Badges { get; set; } = new List<AssignedBadge>();
		public List<AssessmentResult> UserCareerPaths { get; set; } = new List<AssessmentResult>();
		public DateTime DateAssigned { get; set; }
	}
}

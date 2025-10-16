namespace Domain.Entities
{
	public class AssignedSkill : BaseEntity
	{
		public string Name { get; set; }
		public string Category { get; set; }
		public Learner? Learner { get; set; }
		public Guid? LearnerId { get; set; }
		public TeamMember? TeamMember { get; set; }
		public Guid TeamMemberId { get; set; }
		public Skill Skill { get; set; }
		public Guid SkillId { get; set; }
		public List<AssessmentBatch> AssessmentBatches { get; set; }
		public List<AssessmentResult> AssessmentResults { get; set; }
		public List<AssignedBadge> Badges { get; set; }
		public List<AssessmentResult> UserCareerPaths { get; set; }
		public DateTime DateAssigned { get; set; }
	}
}

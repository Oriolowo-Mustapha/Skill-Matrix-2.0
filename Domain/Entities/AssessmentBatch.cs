namespace Domain.Entities
{
	public class AssessmentBatch
	{
		public int Id { get; set; }
		public Guid? LearnerID { get; set; }
		public Guid SkillId { get; set; }
		public Guid? TeamMemberID { get; set; }
		public DateTime DateCreated { get; set; } = DateTime.UtcNow;
		public AssignedSkill AssignedSkill { get; set; } = null!;
		public Learner? Learner { get; set; }
		public TeamMember? TeamMember { get; set; }
		public List<Assessment> Assessments { get; set; } = new List<Assessment>();
		public AssessmentResult AssessmentResult { get; set; } = null!;
	}
}

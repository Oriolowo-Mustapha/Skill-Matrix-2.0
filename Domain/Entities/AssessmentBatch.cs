namespace Domain.Entities
{
	public class AssessmentBatch
	{
		public int Id { get; set; }
		public Guid? LearnerID { get; set; }
		public Guid SkillId { get; set; }
		public Guid? TeamMemberID { get; set; }
		public DateTime DateCreated { get; set; } = DateTime.Now;
		public AssignedSkill AssignedSkill { get; set; }
		public Learner? Learner { get; set; }
		public TeamMember? TeamMember { get; set; }
		public List<Assessment> Assessments { get; set; }
		public AssessmentResult AssessmentResult { get; set; }
	}
}

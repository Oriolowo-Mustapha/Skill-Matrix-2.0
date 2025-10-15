namespace Domain.Entities
{
	public class AssessmentResult : BaseEntity
	{
		public Guid? LearnerID { get; set; }
		public Learner? Learner { get; set; }
		public Guid? TeamMemberID { get; set; }
		public TeamMember? TeamMember { get; set; }
		public Guid SkillId { get; set; }
		public AssignedSkill Skill { get; set; }
		public Guid AssessmentBatchId { get; set; }
		public AssessmentBatch AssessmentBatch { get; set; }
		public int Score { get; set; }
		public int NoOfCorrectAnswers { get; set; }
		public int NoOfWrongAnswers { get; set; }
		public int NoOfUnansweredQuestions { get; set; }
		public int TotalQuestions { get; set; } = 0;
		public DateTime DateCreated { get; set; } = DateTime.Now;
		public DateTime DateModified { get; set; } = DateTime.Now;

	}
}

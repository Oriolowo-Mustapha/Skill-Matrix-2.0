namespace Domain.Entities
{
	public class AssessmentResult : BaseEntity
	{
		public Guid AdminID { get; set; }
		public Admin Admin { get; set; }
		public Guid LearnerID { get; set; }
		public Learner Learner { get; set; }
		public Guid TeamMemberID { get; set; }
		public Team_Members TeamMember { get; set; }
		public Guid SkillId { get; set; }
		public Skill Skill { get; set; }
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

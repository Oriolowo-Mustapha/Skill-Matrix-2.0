using Domain.Enum;

namespace Domain.Entities
{
	public class AssessmentResult : BaseEntity
	{
		public Guid? LearnerID { get; set; }
		public Learner? Learner { get; set; }
		public Guid? TeamMemberID { get; set; }
		public TeamMember? TeamMember { get; set; }
		public Guid SkillId { get; set; }
		public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Novice;
		public AssignedSkill Skill { get; set; } = null!;
		public int AssessmentBatchId { get; set; }
		public AssessmentBatch AssessmentBatch { get; set; } = null!;
		public Guid? ImprovementPlanId { get; set; }
		public ImprovementPlan? ImprovementPlan { get; set; }
		public int Score { get; set; }
		public int NoOfCorrectAnswers { get; set; }
		public int NoOfWrongAnswers { get; set; }
		public int NoOfUnansweredQuestions { get; set; }
		public int TotalQuestions { get; set; }
		public int McqScore { get; set; }
		public int CodingScore { get; set; }
		public string? VerificationStatus { get; set; }
		public string? PlacedProficiencyLevel { get; set; }
		public DateTime DateCreated { get; set; } = DateTime.UtcNow;
		public DateTime DateModified { get; set; } = DateTime.UtcNow;
	}
}
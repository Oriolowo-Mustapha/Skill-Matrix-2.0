namespace Domain.Entities
{
	public class UserResponse : BaseEntity
	{
		public Guid LearnerId { get; set; }
		public Learner Learner { get; set; }

		public Guid TeamMemberId { get; set; }
		public Team_Members TeamMember { get; set; }

		public Guid AssessmentBatchId { get; set; }
		public AssessmentBatch AssessmentBatch { get; set; }

		public Guid AssessmentQuestionId { get; set; }
		public Assessment AssessmentQuestion { get; set; }

		public Guid SelectedOptionId { get; set; }
		public AssessmentOptions SelectedOption { get; set; }

		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		public bool IsCorrect { get; set; }
	}
}
namespace Domain.Entities
{
	public class UserResponse : BaseEntity
	{
		public Guid? LearnerId { get; set; }
		public Learner? Learner { get; set; }

		public Guid? TeamMemberId { get; set; }
		public TeamMember? TeamMember { get; set; }
		public int AssessmentBatchId { get; set; }
		public AssessmentBatch AssessmentBatch { get; set; }
		public int AssessmentQuestionId { get; set; }
		public Assessment AssessmentQuestion { get; set; }

		public int SelectedOptionId { get; set; }
		public AssessmentOptions SelectedOption { get; set; }

		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		public bool IsCorrect { get; set; }
	}
}
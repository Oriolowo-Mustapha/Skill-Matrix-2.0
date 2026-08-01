namespace Domain.Entities
{
	public class Assessment
	{
		public int Id { get; set; }
		public string Questions { get; set; } = string.Empty;
		public string CorrectAnswer { get; set; } = string.Empty;
		public int AssessmentBatchId { get; set; }
		public AssessmentBatch AssessmentBatch { get; set; } = null!;
		public List<AssessmentOptions> AssessmentOptions { get; set; } = new List<AssessmentOptions>();
		public List<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public Domain.Enum.QuestionType QuestionType { get; set; } = Domain.Enum.QuestionType.MultipleChoice;
		public string? ExpectedOutput { get; set; }
		public string? SampleInput { get; set; }
		public string? CodeTemplate { get; set; }
		public string Concept { get; set; } = string.Empty;
	}
}
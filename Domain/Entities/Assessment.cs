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
		public DateTime CreatedAt = DateTime.UtcNow;
	}
}

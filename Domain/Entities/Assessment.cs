namespace Domain.Entities
{
	public class Assessment
	{
		public int Id { get; set; }
		public string Questions { get; set; }
		public string CorrectAnswer { get; set; }
		public Guid AssessmentBatchId { get; set; }
		public AssessmentBatch AssessmentBatch { get; set; }
		public List<AssessmentOptions> AssessmentOptions { get; set; } = new List<AssessmentOptions>();
		public List<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
	}
}

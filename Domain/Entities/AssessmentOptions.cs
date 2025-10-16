namespace Domain.Entities
{
	public class AssessmentOptions
	{
		public int Id { get; set; }
		public string OptionText { get; set; } = string.Empty;
		public int AssessmentId { get; set; }
		public Assessment Assessment { get; set; } = null!;
		public List<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
	}
}

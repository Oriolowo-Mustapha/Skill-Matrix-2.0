namespace Domain.Entities
{
	public class AssessmentOptions
	{
		public int Id { get; set; }
		public string OptionText { get; set; }
		public Guid AssessmentId { get; set; }
		public Assessment Assessment { get; set; }
		public List<UserResponse> UserResponses { get; set; }
	}
}

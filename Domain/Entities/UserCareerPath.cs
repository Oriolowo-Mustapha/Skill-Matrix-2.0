namespace Domain.Entities
{
	public class UserCareerPath
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string ImageUrl { get; set; }
		public Guid LearnerId { get; set; }
		public Learner Learner { get; set; }
		public DateTime DateAssigned { get; set; }
	}
}

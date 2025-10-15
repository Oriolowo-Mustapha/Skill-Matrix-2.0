namespace Domain.Entities
{
	public class UserBadge : BaseEntity
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string ImageUrl { get; set; }
		public Guid LearnerId { get; set; }
		public Learner Learner { get; set; }
		public DateTime DateAwarded { get; set; }
	}
}

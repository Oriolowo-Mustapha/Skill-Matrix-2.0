namespace Domain.Entities
{
	public class AssignedBadge : BaseEntity
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string ImageUrl { get; set; } = string.Empty;
		public Guid? TeamMemberId { get; set; }
		public TeamMember? TeamMember { get; set; }
		public Guid? LearnerID { get; set; }
		public Learner? Learner { get; set; }
		public DateTime DateAwarded { get; set; }
	}
}

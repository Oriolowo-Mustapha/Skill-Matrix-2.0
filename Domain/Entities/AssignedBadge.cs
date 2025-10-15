namespace Domain.Entities
{
	public class AssignedBadge
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string ImageUrl { get; set; }
		public Guid? TeamMemberId { get; set; }
		public TeamMember? TeamMember { get; set; }
		public Guid? LearnerID { get; set; }
		public Learner? Learner { get; set; }
		public DateTime DateAwarded { get; set; }
	}
}

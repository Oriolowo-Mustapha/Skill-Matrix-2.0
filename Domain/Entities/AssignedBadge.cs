namespace Domain.Entities
{
	public class AssignedBadge : BaseEntity
	{
		public Guid BadgeId { get; set; }
		public Badge Badge { get; set; }
		public Guid? TeamMemberId { get; set; }
		public TeamMember? TeamMember { get; set; }
		public Guid? LearnerID { get; set; }
		public Learner? Learner { get; set; }
		public DateTime DateAwarded { get; set; }
	}
}
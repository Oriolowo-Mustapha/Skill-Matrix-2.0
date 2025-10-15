namespace Domain.Entities
{
	public class TeamMemberBadge
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string ImageUrl { get; set; }
		public Guid TeamMemberId { get; set; }
		public Team_Members TeamMember { get; set; }
		public DateTime DateAwarded { get; set; }

	}
}

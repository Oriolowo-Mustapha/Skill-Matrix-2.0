namespace Domain.Entities
{
	public class TeamMemberCareerPath
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string ImageUrl { get; set; }
		public Guid TeamMemberId { get; set; }
		public Team_Members TeamMember { get; set; }
		public DateTime DateAssigned { get; set; }
	}
}

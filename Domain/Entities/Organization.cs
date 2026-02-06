namespace Domain.Entities
{
	public class Organization : BaseEntity
	{
		public string Name { get; set; } = string.Empty;
		public string? ProfilePictureUrl { get; set; }
		public string Description { get; set; } = string.Empty;
		public List<Manager>? Managers { get; set; } = new List<Manager>();
		public List<TeamMember>? TeamMembers { get; set; } = new List<TeamMember>();
		public DateTime DateJoined { get; set; } = DateTime.UtcNow;
	}
}
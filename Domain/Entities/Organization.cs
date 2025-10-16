namespace Domain.Entities
{
	public class Organization : BaseEntity
	{
		public string Name { get; set; } = string.Empty;
		public string? ProfilePictureUrl { get; set; }
		public Guid ManagerId { get; set; }
		public Manager Manager { get; set; } = null!;
		public List<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
		public DateTime DateJoined { get; set; } = DateTime.UtcNow;
	}
}

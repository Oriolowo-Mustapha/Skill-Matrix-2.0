using Domain.Enum;

namespace Domain.Entities
{
	public class Manager : BaseEntity
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? ProfilePictureUrl { get; set; }
		public Roles Roles { get; set; } = Roles.Manager;
		public string PasswordHash { get; set; } = string.Empty;
		public Guid OrganizationId { get; set; }
		public Organization Organization { get; set; } = null!;
		public List<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
		public DateTime DateJoined { get; set; } = DateTime.UtcNow;
	}
}

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
		public string Role { get; set; } = Roles.Manager.ToString();
		public string PasswordHash { get; set; } = string.Empty;
		public bool IsEmailVerified { get; set; } = false;
		public string? EmailVerificationToken { get; set; }
		public DateTime? EmailVerificationTokenExpiry { get; set; }
		public string? PasswordResetToken { get; set; }
		public DateTime? PasswordResetTokenExpiry { get; set; }
		public Guid OrganizationId { get; set; }
		public Organization Organization { get; set; } = null!;
		public List<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
		public DateTime DateJoined { get; set; } = DateTime.UtcNow;
	}
}
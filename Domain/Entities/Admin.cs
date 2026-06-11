using Domain.Enum;

namespace Domain.Entities
{
	public class Admin
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string? PasswordResetToken { get; set; }
		public DateTime? PasswordResetTokenExpiry { get; set; }
		public string Role { get; set; } = Roles.Admin.ToString();
		public DateTime DateJoined { get; set; }
	}
}
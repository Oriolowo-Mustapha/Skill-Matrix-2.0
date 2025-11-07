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
		public Roles Role { get; set; }
		public DateTime DateJoined { get; set; }
	}
}
using Domain.Enum;

namespace Domain.Entities
{
	public class Admin : BaseEntity
	{
		public required string FirstName { get; set; }
		public required string LastName { get; set; }
		public required string Email { get; set; }
		public required string UserName { get; set; }
		public required string PasswordHash { get; set; }
		public Roles Role { get; set; }
		public DateTime DateJoined = DateTime.UtcNow;
	}
}
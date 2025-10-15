using Domain.Enum;

namespace Domain.Entities
{
	public class Manager : BaseEntity
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string ProfilePictureUrl { get; set; }
		public Roles Roles { get; set; } = Roles.Manager;
		public string PasswordHash { get; set; }
		public Guid OrganizationId { get; set; }
		public Organization Organization { get; set; }
		public List<Team_Members> Team_Members { get; set; }
	}
}

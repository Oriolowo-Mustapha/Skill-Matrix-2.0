using Domain.Enum;

namespace Domain.Entities
{
	public class Team_Members : BaseEntity
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public Roles Role { get; set; } = Roles.Team_Members;
		public string ProfilePictureUrl { get; set; }
		public string PasswordHash { get; set; }
		public Guid ManagerId { get; set; }
		public Manager Manager { get; set; }

		public Guid OrganizationId { get; set; }
		public Organization Organization { get; set; }
		public List<TeamMemberBadge> Badges { get; set; }
		public List<TeamMemberCareerPath> CareerPaths { get; set; }
		public List<TeamMemberSkill> teamMemberSkills { get; set; }

	}
}

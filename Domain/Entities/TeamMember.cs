using Domain.Enum;

namespace Domain.Entities
{
	public class TeamMember : BaseEntity
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public Roles Role { get; set; } = Roles.Team_Members;
		public string? ProfilePictureUrl { get; set; }
		public string PasswordHash { get; set; } = string.Empty;
		public Guid ManagerId { get; set; }
		public Manager Manager { get; set; } = null!;

		public Guid OrganizationId { get; set; }
		public Organization Organization { get; set; } = null!;
		public List<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
		public List<AssignedBadge> Badges { get; set; } = new List<AssignedBadge>();
		public List<AssessmentBatch> AssessmentBatches { get; set; } = new List<AssessmentBatch>();
		public List<AssignedCareerPath> CareerPaths { get; set; } = new List<AssignedCareerPath>();
		public List<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();
		public List<AssignedSkill> TeamMemberSkills { get; set; } = new List<AssignedSkill>();
		public DateTime DateJoined { get; set; } = DateTime.UtcNow;
	}
}

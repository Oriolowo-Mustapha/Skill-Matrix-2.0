using Domain.Enum;

namespace Domain.Entities
{
	public class Learner : BaseEntity
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? ProfilePictureUrl { get; set; }
		public string PasswordHash { get; set; } = string.Empty;
		public Roles Role { get; set; } = Roles.Learner;
		public List<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
		public List<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();
		public List<AssessmentBatch> AssessmentBatches { get; set; } = new List<AssessmentBatch>();
		public List<AssignedBadge> Badges { get; set; } = new List<AssignedBadge>();
		public List<AssignedCareerPath> LearnerCareerPaths { get; set; } = new List<AssignedCareerPath>();
		public List<AssignedSkill> LearnerSkills { get; set; } = new List<AssignedSkill>();
		public DateTime DateJoined { get; set; } = DateTime.UtcNow;
	}
}

using Domain.Enum;

namespace Domain.Entities
{
	public class Learner : BaseEntity
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string? ProfilePictureUrl { get; set; }
		public string PasswordHash { get; set; }
		public Roles Role { get; set; } = Roles.Learner;
		public List<UserResponse> UserResponses { get; set; }
		public List<AssessmentResult> AssessmentResults { get; set; }
		public List<AssessmentBatch> AssessmentBatches { get; set; }
		public List<AssignedBadge> Badges { get; set; }
		public List<AssignedCareerPath> LearnerCareerPaths { get; set; }
		public List<AssignedSkill> LearnerSkills { get; set; }
	}
}

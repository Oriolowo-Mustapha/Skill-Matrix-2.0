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
		public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Novice;
		public string Role { get; set; } = Roles.Learner.ToString();
		public bool IsEmailVerified { get; set; } = false;
		public string? EmailVerificationToken { get; set; }
		public DateTime? EmailVerificationTokenExpiry { get; set; }
		public string? PasswordResetToken { get; set; }
		public DateTime? PasswordResetTokenExpiry { get; set; }
		public List<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
		public List<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();
		public List<AssessmentBatch> AssessmentBatches { get; set; } = new List<AssessmentBatch>();
		public List<AssignedBadge> Badges { get; set; } = new List<AssignedBadge>();
		public List<AssignedCareerPath> LearnerCareerPaths { get; set; } = new List<AssignedCareerPath>();
		public List<AssignedSkill> LearnerSkills { get; set; } = new List<AssignedSkill>();
		public DateTime DateJoined { get; set; } = DateTime.UtcNow;
		public int TotalPoints { get; set; } = 0;
	}
}
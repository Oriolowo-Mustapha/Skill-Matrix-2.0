using Domain.Enum;

namespace Domain.Entities
{
	public class Learner : BaseEntity
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string ProfilePictureUrl { get; set; }
		public string PasswordHash { get; set; }
		public Roles Role { get; set; } = Roles.Learner;
		public List<AssessmentResult> AssessmentResults { get; set; }
		public List<UserBadge> Badges { get; set; }
		public List<UserCareerPath> LearnerCareerPaths { get; set; }
		public List<LearnerSkill> UserSkills { get; set; }
	}
}

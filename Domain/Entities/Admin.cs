using Domain.Enum;

namespace Domain.Entities
{
	public class Admin : BaseEntity
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public Roles Role { get; set; } = Roles.Admin;
		public List<Organization> Organizations { get; set; }
		public List<Learner> Learners { get; set; }
		public List<AssessmentResult> AssessmentResults { get; set; }
	}
}

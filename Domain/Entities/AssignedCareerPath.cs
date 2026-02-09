namespace Domain.Entities
{
	public class AssignedCareerPath : BaseEntity
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string? ImageUrl { get; set; }
		public Guid? TeamMemberId { get; set; }
		public TeamMember? TeamMember { get; set; }
		public Guid? LearnerId { get; set; }
		public Learner? Learner { get; set; }

        public Guid CareerPathId { get; set; }
        public CareerPath CareerPath { get; set; } = null!;

		public DateTime DateAssigned { get; set; }
	}
}
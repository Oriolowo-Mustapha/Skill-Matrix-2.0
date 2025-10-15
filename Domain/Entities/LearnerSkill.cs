namespace Domain.Entities
{
	public class LearnerSkill : BaseEntity
	{
		public string Name { get; set; }
		public string Category { get; set; }
		public List<AssessmentBatch> AssessmentBatches { get; set; }
		public List<AssessmentResult> AssessmentResults { get; set; }
		public List<UserBadge> Badges { get; set; }
		public List<UserCareerPath> UserCareerPaths { get; set; }
	}
}

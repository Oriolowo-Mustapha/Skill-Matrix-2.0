namespace Domain.Entities
{
	public class TeamMemberSkill : BaseEntity
	{
		public string Name { get; set; }
		public string Category { get; set; }
		public List<AssessmentBatch> AssessmentBatches { get; set; }
		public List<AssessmentResult> AssessmentResults { get; set; }
		public List<TeamMemberBadge> Badges { get; set; }
		public List<TeamMemberCareerPath> UserCareerPaths { get; set; }
	}
}

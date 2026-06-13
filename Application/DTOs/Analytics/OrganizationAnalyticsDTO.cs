namespace Application.DTOs.Analytics
{
	public class SkillDistributionDTO
	{
		public string SkillName { get; set; } = string.Empty;
		public int NoviceCount { get; set; }
		public int BegineerCount { get; set; }
		public int IntermediateCount { get; set; }
		public int ProficientCount { get; set; }
		public int ExpertCount { get; set; }
	}

	public class TeamGrowthMetricDTO
	{
		public string Month { get; set; } = string.Empty;
		public int AssessmentsCompleted { get; set; }
		public double AverageScore { get; set; }
	}

	public class OrganizationAnalyticsDTO
	{
		public Guid OrganizationId { get; set; }
		public int TotalMembers { get; set; }
		public int TotalAssessmentsCompleted { get; set; }
		public double AverageProficiencyScore { get; set; }
		public List<string> TopSkills { get; set; } = new List<string>();
		public List<SkillDistributionDTO> SkillDistributions { get; set; } = new List<SkillDistributionDTO>();
		public List<TeamGrowthMetricDTO> GrowthMetrics { get; set; } = new List<TeamGrowthMetricDTO>();
	}
}

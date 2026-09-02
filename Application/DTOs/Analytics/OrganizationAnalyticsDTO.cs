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

	public class MemberSummaryDTO
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string ProfilePictureUrl { get; set; } = string.Empty;
		public int TotalPoints { get; set; }
		public int AssessmentsCompleted { get; set; }
		public double AverageScore { get; set; }
		public string ProficiencyLevel { get; set; } = string.Empty;
	}

	public class ActivityEventDTO
	{
		public string Type { get; set; } = string.Empty; // "assessment", "plan", "task", "badge", "member"
		public string Action { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string MemberName { get; set; } = string.Empty;
		public Guid? MemberId { get; set; }
		public string? SkillOrBadgeName { get; set; }
		public DateTime Date { get; set; }
	}

	public class OrganizationAnalyticsDTO
	{
		public Guid OrganizationId { get; set; }
		public int TotalMembers { get; set; }
		public int TotalAssessmentsCompleted { get; set; }
		public double AverageProficiencyScore { get; set; }
		public int ActiveImprovementPlansCount { get; set; }
		public int BadgesAwardedCount { get; set; }
		public int SkillGapsCount { get; set; }
		public int MasteredSkillsCount { get; set; }
		public List<string> TopSkills { get; set; } = new List<string>();
		public List<SkillDistributionDTO> SkillDistributions { get; set; } = new List<SkillDistributionDTO>();
		public List<TeamGrowthMetricDTO> GrowthMetrics { get; set; } = new List<TeamGrowthMetricDTO>();
		public List<MemberSummaryDTO> TopMembers { get; set; } = new List<MemberSummaryDTO>();
		public List<MemberSummaryDTO> WeakMembers { get; set; } = new List<MemberSummaryDTO>();
		public List<ActivityEventDTO> RecentActivity { get; set; } = new List<ActivityEventDTO>();
	}
}

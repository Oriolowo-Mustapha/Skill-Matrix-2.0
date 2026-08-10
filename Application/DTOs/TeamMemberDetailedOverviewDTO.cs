using System;
using System.Collections.Generic;

namespace Application.DTOs
{
	public class TeamMemberDetailedOverviewDTO
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public string? ProfilePictureUrl { get; set; }
		public DateTime DateJoined { get; set; }
		public int TotalPoints { get; set; }

		// Skills Summary
		public int TotalAssignedSkills { get; set; }
		public int MasteredSkillsCount { get; set; }
		public int InProgressSkillsCount { get; set; }
		public List<TeamMemberSkillOverviewDTO> Skills { get; set; } = new();

		// Career Path Progress
		public List<TeamMemberCareerPathOverviewDTO> CareerPaths { get; set; } = new();

		// Assessment History & Performance
		public int TotalAssessmentsTaken { get; set; }
		public double AverageAssessmentScore { get; set; }
		public List<TeamMemberAssessmentOverviewDTO> RecentAssessments { get; set; } = new();

		// Improvement Plans
		public List<TeamMemberImprovementPlanOverviewDTO> ImprovementPlans { get; set; } = new();
	}

	public class TeamMemberSkillOverviewDTO
	{
		public Guid SkillId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public string ProficiencyLevel { get; set; } = string.Empty;
		public bool IsFullyMastered { get; set; }
		public DateTime DateAssigned { get; set; }
	}

	public class TeamMemberCareerPathOverviewDTO
	{
		public Guid CareerPathId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public double ProgressPercentage { get; set; }
		public DateTime DateAssigned { get; set; }
	}

	public class TeamMemberAssessmentOverviewDTO
	{
		public Guid AssessmentResultId { get; set; }
		public string SkillName { get; set; } = string.Empty;
		public int Score { get; set; }
		public int TotalQuestions { get; set; }
		public int NoOfCorrectAnswers { get; set; }
		public string AchievedLevel { get; set; } = string.Empty;
		public DateTime DateTaken { get; set; }
	}

	public class TeamMemberImprovementPlanOverviewDTO
	{
		public Guid Id { get; set; }
		public string FocusArea { get; set; } = string.Empty;
		public string GeneratedSummary { get; set; } = string.Empty;
		public DateTime DateGenerated { get; set; }
		public int TotalTasks { get; set; }
		public int CompletedTasks { get; set; }
	}
}

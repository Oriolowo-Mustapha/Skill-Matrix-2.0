using System;
using System.Collections.Generic;

namespace Application.DTOs
{
	public class MyOverviewDTO
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public string? ProfilePictureUrl { get; set; }
		public DateTime DateJoined { get; set; }

		// Gamification
		public int TotalPoints { get; set; }
		public int CurrentStreak { get; set; }
		public StreakDTO? Streak { get; set; }
		public int? Level { get; set; }
		public string? LevelTitle { get; set; }
		public int BadgeCount { get; set; }
		public DateTime LastActivityDate { get; set; }
		public List<DailyQuestDTO> DailyQuests { get; set; } = new();
		public List<RecentActivityDTO> RecentActivity { get; set; } = new();

		// Skills Summary
		public int TotalAssignedSkills { get; set; }
		public int MasteredSkillsCount { get; set; }
		public int InProgressSkillsCount { get; set; }
		public List<MySkillOverviewDTO> Skills { get; set; } = new();

		// Career Path Progress
		public List<MyCareerPathOverviewDTO> CareerPaths { get; set; } = new();

		// Assessment History & Performance
		public int TotalAssessmentsTaken { get; set; }
		public double AverageAssessmentScore { get; set; }
		public List<MyAssessmentOverviewDTO> RecentAssessments { get; set; } = new();

		// Improvement Plans
		public List<MyImprovementPlanOverviewDTO> ImprovementPlans { get; set; } = new();
	}

	public class DailyQuestDTO
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public int Target { get; set; }
		public int Progress { get; set; }
		public int XpReward { get; set; }
		public bool Completed { get; set; }
	}

	public class RecentActivityDTO
	{
		public Guid Id { get; set; }
		public string ActivityType { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public int PointsEarned { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public class MySkillOverviewDTO
	{
		public Guid SkillId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public string ProficiencyLevel { get; set; } = string.Empty;
		public bool IsFullyMastered { get; set; }
		public DateTime DateAssigned { get; set; }
	}

	public class MyCareerPathOverviewDTO
	{
		public Guid CareerPathId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public double ProgressPercentage { get; set; }
		public DateTime DateAssigned { get; set; }
	}

	public class MyAssessmentOverviewDTO
	{
		public Guid AssessmentResultId { get; set; }
		public string SkillName { get; set; } = string.Empty;
		public int Score { get; set; }
		public int TotalQuestions { get; set; }
		public int NoOfCorrectAnswers { get; set; }
		public string AchievedLevel { get; set; } = string.Empty;
		public DateTime DateTaken { get; set; }
	}

	public class MyImprovementPlanOverviewDTO
	{
		public Guid Id { get; set; }
		public string FocusArea { get; set; } = string.Empty;
		public string GeneratedSummary { get; set; } = string.Empty;
		public DateTime DateGenerated { get; set; }
		public int TotalTasks { get; set; }
		public int CompletedTasks { get; set; }
	}
}
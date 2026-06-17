namespace Application.DTOs
{
	public record AssessmentResultDTO
	{
		public Guid Id { get; set; }
		public string SkillName { get; set; } = string.Empty;
		public int Score { get; set; }
		public int NoOfCorrectAnswers { get; set; }
		public int NoOfWrongAnswers { get; set; }
		public int TotalQuestions { get; set; }
		public string ProficiencyLevel { get; set; } = string.Empty;
		public DateTime DateCompleted { get; set; }
		public Guid ImprovementPlanId { get; set; }
		public bool Passed { get; set; }
		public int PassingScore { get; set; }
		public bool LevelUp { get; set; } = false;
		public string NewProficiencyLevel { get; set; } = string.Empty;
		public bool BadgeUnlocked { get; set; } = false;
		public string BadgeTitle { get; set; } = string.Empty;
	}
}

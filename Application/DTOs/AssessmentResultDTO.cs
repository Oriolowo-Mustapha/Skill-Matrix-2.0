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
	}
}

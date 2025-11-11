namespace Application.DTOs
{
	public record TeamMemberPerformanceDTO
	{
		public UserDTO TeamMember { get; set; } = null!;
		public List<SkillDTO> Skills { get; set; } = new List<SkillDTO>();
		public List<AssessmentResultSummaryDTO> AssessmentResultSummaryDTO { get; set; } = new List<AssessmentResultSummaryDTO>();
	}

	public record AssessmentResultSummaryDTO
	{
		public Guid Id { get; set; }
		public string SkillName { get; set; } = string.Empty;
		public int Score { get; set; }
		public DateTime DateCompleted { get; set; }
	}

}

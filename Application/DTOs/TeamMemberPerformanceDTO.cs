namespace Application.DTOs
{
	public record TeamMemberPerformanceDTO
	{
		public UserDTO TeamMember { get; set; }
		public List<SkillDTO> Skills { get; set; }
		public List<AssessmentResultSummaryDTO> AssessmentResultSummaryDTO { get; set; }
	}

	public record AssessmentResultSummaryDTO
	{
		public Guid Id { get; set; }
		public string SkillName { get; set; }
		public int Score { get; set; }
		public DateTime DateCompleted { get; set; }
	}

}

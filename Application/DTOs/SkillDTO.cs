namespace Application.DTOs
{
	public record SkillDTO
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public string ProficiencyLevel { get; set; } = string.Empty;
		public bool IsFullyMastered { get; set; }
		public DateTime DateAssigned { get; set; }
	}
}

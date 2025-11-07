namespace Application.DTOs
{
	public record SkillDTO
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public string ProficiencyLevel { get; set; }
		public DateTime DateAssigned { get; set; }
	}
}

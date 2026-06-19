namespace Application.DTOs
{
	public record CareerPathTrackDTO
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string? IconUrl { get; set; }
		public Guid CareerPathId { get; set; }
		public List<SkillDTO> Skills { get; set; } = new();
	}
}

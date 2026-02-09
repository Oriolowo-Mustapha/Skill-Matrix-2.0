namespace Application.DTOs
{
	public record BadgeDTO
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string IconUrl { get; set; } = string.Empty;
		public string Criteria { get; set; } = string.Empty;
		public string ProficiencyLevel { get; set; } = string.Empty;
	}

	public record CreateBadgeRequest
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string IconUrl { get; set; } = string.Empty;
		public string Criteria { get; set; } = string.Empty;
		public string ProficiencyLevel { get; set; } = string.Empty;

	}
}

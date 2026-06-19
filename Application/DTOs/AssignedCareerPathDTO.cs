namespace Application.DTOs
{
	public record AssignedCareerPathDTO
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string? ImageUrl { get; set; }
		public Guid CareerPathId { get; set; }
		public Guid? CareerPathTrackId { get; set; }
		public string? TrackName { get; set; }
		public DateTime DateAssigned { get; set; }
		public int ProgressPercentage { get; set; }
	}
}
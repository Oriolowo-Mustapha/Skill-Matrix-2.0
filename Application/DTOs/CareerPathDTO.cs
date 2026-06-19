using Application.DTOs;

namespace Application.DTOs
{
	public record CareerPathDTO
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string IconURL { get; set; } = string.Empty;
		public DateTime DateAdded { get; set; }
		public List<SkillDTO> Skills { get; set; } = new();
		public List<CareerPathTrackDTO> Tracks { get; set; } = new();
	}
}
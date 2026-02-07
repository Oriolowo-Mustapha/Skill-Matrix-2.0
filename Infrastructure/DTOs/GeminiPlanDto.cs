namespace Infrastructure.DTOs
{
	public class GeminiPlanDto
	{
		public string Summary { get; set; } = string.Empty;
		public string FocusAreas { get; set; } = string.Empty;
		public List<GeminiResourceDto> Resources { get; set; } = new List<GeminiResourceDto>();
	}
}
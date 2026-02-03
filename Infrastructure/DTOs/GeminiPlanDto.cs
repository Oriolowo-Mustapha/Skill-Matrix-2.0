namespace Infrastructure.DTOs
{
	public class GeminiPlanDto
	{
		public string Summary { get; set; }
		public string FocusAreas { get; set; }
		public List<GeminiResourceDto> Resources { get; set; }
	}
}

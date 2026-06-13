namespace Infrastructure.DTOs
{
	public class GeminiTaskDto
	{
		public string Concept { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string ResourceTitle { get; set; } = string.Empty; // Maps task to resource
	}
}

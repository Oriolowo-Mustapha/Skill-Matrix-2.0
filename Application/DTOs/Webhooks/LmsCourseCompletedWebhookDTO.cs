namespace Application.DTOs.Webhooks
{
	public class LmsCourseCompletedWebhookDTO
	{
		public string UserEmail { get; set; } = string.Empty;
		public string ProviderName { get; set; } = string.Empty; // e.g. "Coursera", "Udemy"
		public string SkillName { get; set; } = string.Empty;
		public string CourseTitle { get; set; } = string.Empty;
		public DateTime CompletionDate { get; set; }
	}
}

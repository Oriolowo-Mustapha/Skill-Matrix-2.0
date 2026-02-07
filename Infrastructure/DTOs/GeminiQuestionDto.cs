namespace Infrastructure.DTOs
{
	public class GeminiQuestionDto
	{
		public string QuestionText { get; set; } = string.Empty;
		public List<string> Options { get; set; } = new List<string>();
		public string CorrectAnswer { get; set; } = string.Empty;
	}
}
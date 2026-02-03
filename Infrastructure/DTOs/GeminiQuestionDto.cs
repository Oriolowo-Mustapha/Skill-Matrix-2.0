namespace Infrastructure.DTOs
{
	public class GeminiQuestionDto
	{
		public string QuestionText { get; set; }
		public List<string> Options { get; set; }
		public string CorrectAnswer { get; set; }
	}
}

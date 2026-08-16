namespace Infrastructure.DTOs
{
	public class GeminiQuestionDto
	{
		public string QuestionText { get; set; } = string.Empty;
		public List<string> Options { get; set; } = new List<string>();
		public string CorrectAnswer { get; set; } = string.Empty;
		public string QuestionType { get; set; } = "MultipleChoice"; // "MultipleChoice" or "Coding"
		public string? ExpectedOutput { get; set; }
		public string? SampleInput { get; set; }
		public string? CodeTemplate { get; set; }
		public string? FunctionName { get; set; }
		public List<Application.DTOs.Assessments.TestCaseItem>? TestCases { get; set; } = new List<Application.DTOs.Assessments.TestCaseItem>();
		public string Concept { get; set; } = string.Empty;
	}
}
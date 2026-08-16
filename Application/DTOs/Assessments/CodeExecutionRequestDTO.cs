using System.Collections.Generic;

namespace Application.DTOs.Assessments
{
	public class CodeExecutionRequestDTO
	{
		public string Language { get; set; } = string.Empty; // e.g. "csharp", "python"
		public string SourceCode { get; set; } = string.Empty;
		public string ExpectedOutput { get; set; } = string.Empty;
		public string? SampleInput { get; set; }
		public string? FunctionName { get; set; }
		public List<TestCaseItem>? TestCases { get; set; }
	}
}

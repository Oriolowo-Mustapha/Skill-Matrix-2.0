using System.Collections.Generic;

namespace Application.DTOs.Assessments
{
	public class CodeExecutionResponseDTO
	{
		public bool IsSuccess { get; set; }
		public string ConsoleOutput { get; set; } = string.Empty;
		public string ErrorMessage { get; set; } = string.Empty;
		public int PassedCount { get; set; }
		public int TotalCount { get; set; }
		public List<TestCaseResult> TestResults { get; set; } = new List<TestCaseResult>();
		public string? GeneratedSource { get; set; }
		public bool IsHarnessApplied { get; set; }
		public int CompilationLineOffset { get; set; }
	}
}

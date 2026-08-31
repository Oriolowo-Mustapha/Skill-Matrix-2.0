using Application.DTOs.Assessments;
using System.Collections.Generic;

namespace Application.Interfaces.Service
{
	public interface ICodeHarnessBuilder
	{
		bool Supports(string language);
		string BuildHarness(string userCode, string functionName, List<TestCaseItem> testCases);
		int CompilationLineOffset { get; }
	}
}

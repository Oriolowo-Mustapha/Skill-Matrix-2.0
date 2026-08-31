using Application.DTOs.Assessments;
using System.Collections.Generic;

namespace Application.Interfaces.Service
{
	public interface ICodeHarnessFactory
	{
		string BuildHarness(string language, string userCode, string functionName, List<TestCaseItem> testCases, out int lineOffset);
		bool HasHarness(string language);
	}
}

using Application.DTOs.Assessments;
using Application.Interfaces.Service;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Implementation.Services.CodeHarnesses
{
	public class CodeHarnessFactory : ICodeHarnessFactory
	{
		private readonly IEnumerable<ICodeHarnessBuilder> _builders;

		public CodeHarnessFactory(IEnumerable<ICodeHarnessBuilder> builders)
		{
			_builders = builders;
		}

		public bool HasHarness(string language)
		{
			return _builders.Any(b => b.Supports(language));
		}

		public string BuildHarness(string language, string userCode, string functionName, List<TestCaseItem> testCases, out int lineOffset)
		{
			var builder = _builders.FirstOrDefault(b => b.Supports(language));
			if (builder != null)
			{
				lineOffset = builder.CompilationLineOffset;
				return builder.BuildHarness(userCode, functionName, testCases);
			}

			lineOffset = 0;
			return userCode;
		}
	}
}

using Application.DTOs.Assessments;
using Application.Interfaces.Service;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Implementation.Services.CodeHarnesses
{
	public class CppHarnessBuilder : ICodeHarnessBuilder
	{
		public int CompilationLineOffset => 10;

		public bool Supports(string language)
		{
			if (string.IsNullOrWhiteSpace(language)) return false;
			var lang = language.Trim().ToLower();
			return lang is "cpp" or "c++" or "cplusplus";
		}

		public string BuildHarness(string userCode, string functionName, List<TestCaseItem> testCases)
		{
			var sb = new StringBuilder();
			sb.AppendLine("#include <iostream>");
			sb.AppendLine("#include <vector>");
			sb.AppendLine("#include <string>");
			sb.AppendLine("#include <sstream>");
			sb.AppendLine("#include <algorithm>");
			sb.AppendLine("using namespace std;");
			sb.AppendLine();
			sb.AppendLine(userCode);
			sb.AppendLine();
			sb.AppendLine("struct TestCase {");
			sb.AppendLine("    string input;");
			sb.AppendLine("    string expected;");
			sb.AppendLine("    bool isHidden;");
			sb.AppendLine("};");
			sb.AppendLine();
			sb.AppendLine("int main() {");
			sb.AppendLine("    vector<TestCase> testCases = {");
			foreach (var tc in testCases)
			{
				sb.AppendLine($"        {{\"{EscapeCppString(tc.Input)}\", \"{EscapeCppString(tc.ExpectedOutput)}\", {(tc.IsHidden ? "true" : "false")}}},");
			}
			sb.AppendLine("    };");
			sb.AppendLine();
			sb.AppendLine("    int passedCount = 0;");
			sb.AppendLine("    Solution solver;");
			sb.AppendLine("    for (size_t idx = 0; idx < testCases.size(); idx++) {");
			sb.AppendLine("        try {");
			var safeFn = string.IsNullOrWhiteSpace(functionName) ? "solve" : functionName.Trim();
			sb.AppendLine("            int inpNum = 0;");
			sb.AppendLine("            stringstream ss(testCases[idx].input);");
			sb.AppendLine("            ss >> inpNum;");
			sb.AppendLine($"            auto result = solver.{safeFn}(inpNum);");
			sb.AppendLine("            string actual = to_string(result);");
			sb.AppendLine("            bool isMatch = (actual == testCases[idx].expected);");
			sb.AppendLine("            if (isMatch) passedCount++;");
			sb.AppendLine("            cout << \"[TC_RESULT:\" << idx << \"|PASSED:\" << (isMatch ? \"True\" : \"False\") << \"|ACTUAL:\" << actual << \"|HIDDEN:\" << (testCases[idx].isHidden ? \"True\" : \"False\") << \"]\\n\";");
			sb.AppendLine("        } catch (const exception& e) {");
			sb.AppendLine("            cout << \"[TC_RESULT:\" << idx << \"|PASSED:False|ACTUAL:Runtime Error: \" << e.what() << \"|HIDDEN:\" << (testCases[idx].isHidden ? \"True\" : \"False\") << \"]\\n\";");
			sb.AppendLine("        }");
			sb.AppendLine("    }");
			sb.AppendLine("    cout << \"[TC_SUMMARY:PASSED=\" << passedCount << \"|TOTAL=\" << testCases.size() << \"]\\n\";");
			sb.AppendLine("    return 0;");
			sb.AppendLine("}");
			return sb.ToString();
		}

		private static string EscapeCppString(string? input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
		}
	}
}

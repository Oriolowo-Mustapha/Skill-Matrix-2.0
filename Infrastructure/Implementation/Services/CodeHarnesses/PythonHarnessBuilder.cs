using Application.DTOs.Assessments;
using Application.Interfaces.Service;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Implementation.Services.CodeHarnesses
{
	public class PythonHarnessBuilder : ICodeHarnessBuilder
	{
		public int CompilationLineOffset => 0;

		public bool Supports(string language)
		{
			if (string.IsNullOrWhiteSpace(language)) return false;
			var lang = language.Trim().ToLower();
			return lang is "python" or "py" or "python3";
		}

		public string BuildHarness(string userCode, string functionName, List<TestCaseItem> testCases)
		{
			var sb = new StringBuilder();
			sb.AppendLine(userCode);
			sb.AppendLine();
			sb.AppendLine("import sys");
			sb.AppendLine("import json");
			sb.AppendLine();
			sb.AppendLine("def _parse_py_arg(raw):");
			sb.AppendLine("    raw = str(raw).strip()");
			sb.AppendLine("    try:");
			sb.AppendLine("        return json.loads(raw)");
			sb.AppendLine("    except Exception:");
			sb.AppendLine("        if raw.lower() == 'true': return True");
			sb.AppendLine("        if raw.lower() == 'false': return False");
			sb.AppendLine("        try:");
			sb.AppendLine("            return int(raw)");
			sb.AppendLine("        except ValueError:");
			sb.AppendLine("            try:");
			sb.AppendLine("                return float(raw)");
			sb.AppendLine("            except ValueError:");
			sb.AppendLine("                return raw.strip('\"')");
			sb.AppendLine();
			sb.AppendLine("test_cases = [");
			foreach (var tc in testCases)
			{
				sb.AppendLine($"    ('{EscapePythonString(tc.Input)}', '{EscapePythonString(tc.ExpectedOutput)}', { (tc.IsHidden ? "True" : "False") }),");
			}
			sb.AppendLine("]");
			sb.AppendLine("passed = 0");
			sb.AppendLine("for idx, (inp, exp, hidden) in enumerate(test_cases):");
			sb.AppendLine("    try:");
			var safeFn = string.IsNullOrWhiteSpace(functionName) ? "solve" : functionName.Trim();
			sb.AppendLine("        fn = None");
			sb.AppendLine("        if 'Solution' in globals():");
			sb.AppendLine($"            fn = getattr(Solution, '{safeFn}', None) or getattr(Solution(), '{safeFn}', None)");
			sb.AppendLine("        if not fn:");
			sb.AppendLine($"            fn = globals().get('{safeFn}')");
			sb.AppendLine("        if not fn:");
			sb.AppendLine("            # Fallback to first user-defined callable");
			sb.AppendLine("            for k, v in globals().items():");
			sb.AppendLine("                if callable(v) and not k.startswith('_') and k not in ('test_cases', 'json', 'sys'):");
			sb.AppendLine("                    fn = v; break");
			sb.AppendLine("        if not fn: raise Exception('Target function not found')");
			sb.AppendLine();
			sb.AppendLine("        parsed_arg = _parse_py_arg(inp)");
			sb.AppendLine("        if isinstance(parsed_arg, list) and not isinstance(parsed_arg, str):");
			sb.AppendLine("            val = fn(parsed_arg)");
			sb.AppendLine("        else:");
			sb.AppendLine("            val = fn(parsed_arg)");
			sb.AppendLine("        actual = str(val) if val is not None else 'null'");
			sb.AppendLine("        ok = str(actual).strip().lower() == str(exp).strip().lower()");
			sb.AppendLine("        if ok: passed += 1");
			sb.AppendLine("        print(f'[TC_RESULT:{idx}|PASSED:{ok}|ACTUAL:{actual}|HIDDEN:{hidden}]')");
			sb.AppendLine("    except Exception as e:");
			sb.AppendLine("        print(f'[TC_RESULT:{idx}|PASSED:False|ACTUAL:Runtime Error: {e}|HIDDEN:{hidden}]')");
			sb.AppendLine("print(f'[TC_SUMMARY:PASSED={passed}|TOTAL={len(test_cases)}]')");
			return sb.ToString();
		}

		private static string EscapePythonString(string? input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			return input.Replace("\\", "\\\\").Replace("'", "\\'");
		}
	}
}

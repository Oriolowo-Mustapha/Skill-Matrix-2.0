using Application.DTOs.Assessments;
using Application.Interfaces.Service;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Implementation.Services.CodeHarnesses
{
	public class JavaScriptHarnessBuilder : ICodeHarnessBuilder
	{
		public int CompilationLineOffset => 0;

		public bool Supports(string language)
		{
			if (string.IsNullOrWhiteSpace(language)) return false;
			var lang = language.Trim().ToLower();
			return lang is "javascript" or "js" or "node";
		}

		public string BuildHarness(string userCode, string functionName, List<TestCaseItem> testCases)
		{
			var sb = new StringBuilder();
			sb.AppendLine(userCode);
			sb.AppendLine();
			sb.AppendLine("const _parseJsArg = (raw) => {");
			sb.AppendLine("  const trimmed = String(raw).trim();");
			sb.AppendLine("  try { return JSON.parse(trimmed); } catch (_) {}");
			sb.AppendLine("  if (trimmed.toLowerCase() === 'true') return true;");
			sb.AppendLine("  if (trimmed.toLowerCase() === 'false') return false;");
			sb.AppendLine("  if (!isNaN(trimmed) && trimmed !== '') return Number(trimmed);");
			sb.AppendLine("  return trimmed.replace(/^[\"']|[\"']$/g, '');");
			sb.AppendLine("};");
			sb.AppendLine();
			sb.AppendLine("const testCases = [");
			foreach (var tc in testCases)
			{
				sb.AppendLine($"  {{ input: '{EscapeJsString(tc.Input)}', expected: '{EscapeJsString(tc.ExpectedOutput)}', isHidden: { (tc.IsHidden ? "true" : "false") } }},");
			}
			sb.AppendLine("];");
			sb.AppendLine("let passed = 0;");
			sb.AppendLine("testCases.forEach((tc, idx) => {");
			sb.AppendLine("  try {");
			var safeFn = string.IsNullOrWhiteSpace(functionName) ? "solve" : functionName.Trim();
			sb.AppendLine($"    let fn = null;");
			sb.AppendLine($"    if (typeof {safeFn} === 'function') fn = {safeFn};");
			sb.AppendLine($"    else if (typeof Solution !== 'undefined' && typeof Solution.{safeFn} === 'function') fn = Solution.{safeFn};");
			sb.AppendLine($"    else if (typeof Solution !== 'undefined' && typeof new Solution().{safeFn} === 'function') fn = (arg) => new Solution().{safeFn}(arg);");
			sb.AppendLine("    if (!fn) throw new Error('Target function not found.');");
			sb.AppendLine();
			sb.AppendLine("    const arg = _parseJsArg(tc.input);");
			sb.AppendLine("    const res = fn(arg);");
			sb.AppendLine("    const actual = res !== null && res !== undefined ? String(res) : 'null';");
			sb.AppendLine("    const ok = actual.trim().toLowerCase() === String(tc.expected).trim().toLowerCase();");
			sb.AppendLine("    if (ok) passed++;");
			sb.AppendLine("    console.log(`[TC_RESULT:${idx}|PASSED:${ok}|ACTUAL:${actual}|HIDDEN:${tc.isHidden}]`);");
			sb.AppendLine("  } catch (err) {");
			sb.AppendLine("    console.log(`[TC_RESULT:${idx}|PASSED:false|ACTUAL:Runtime Error: ${err.message}|HIDDEN:${tc.isHidden}]`);");
			sb.AppendLine("  }");
			sb.AppendLine("});");
			sb.AppendLine("console.log(`[TC_SUMMARY:PASSED=${passed}|TOTAL=${testCases.length}]`);");
			return sb.ToString();
		}

		private static string EscapeJsString(string? input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			return input.Replace("\\", "\\\\").Replace("'", "\\'");
		}
	}
}

using Application.DTOs.Assessments;
using Application.Interfaces.Service;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Implementation.Services.CodeHarnesses
{
	public class TypeScriptHarnessBuilder : ICodeHarnessBuilder
	{
		public int CompilationLineOffset => 0;

		public bool Supports(string language)
		{
			if (string.IsNullOrWhiteSpace(language)) return false;
			var lang = language.Trim().ToLower();
			return lang is "typescript" or "ts";
		}

		public string BuildHarness(string userCode, string functionName, List<TestCaseItem> testCases)
		{
			var sb = new StringBuilder();
			sb.AppendLine(userCode);
			sb.AppendLine();
			sb.AppendLine("const _parseTsArg = (raw: any): any => {");
			sb.AppendLine("  const trimmed = String(raw).trim();");
			sb.AppendLine("  try { return JSON.parse(trimmed); } catch (_) {}");
			sb.AppendLine("  if (trimmed.toLowerCase() === 'true') return true;");
			sb.AppendLine("  if (trimmed.toLowerCase() === 'false') return false;");
			sb.AppendLine("  if (!isNaN(Number(trimmed)) && trimmed !== '') return Number(trimmed);");
			sb.AppendLine("  return trimmed.replace(/^[\"']|[\"']$/g, '');");
			sb.AppendLine("};");
			sb.AppendLine();
			sb.AppendLine("const testCases = [");
			foreach (var tc in testCases)
			{
				sb.AppendLine($"  {{ input: '{EscapeTsString(tc.Input)}', expected: '{EscapeTsString(tc.ExpectedOutput)}', isHidden: { (tc.IsHidden ? "true" : "false") } }},");
			}
			sb.AppendLine("];");
			sb.AppendLine("let passed = 0;");
			sb.AppendLine("testCases.forEach((tc, idx) => {");
			sb.AppendLine("  try {");
			var safeFn = string.IsNullOrWhiteSpace(functionName) ? "solve" : functionName.Trim();
			sb.AppendLine($"    let fn: any = null;");
			sb.AppendLine($"    if (typeof (globalThis as any).{safeFn} === 'function') fn = (globalThis as any).{safeFn};");
			sb.AppendLine($"    else if (typeof (globalThis as any).Solution !== 'undefined' && typeof (globalThis as any).Solution.{safeFn} === 'function') fn = (globalThis as any).Solution.{safeFn};");
			sb.AppendLine($"    else if (typeof (globalThis as any).Solution !== 'undefined') fn = (arg: any) => new (globalThis as any).Solution().{safeFn}(arg);");
			sb.AppendLine("    if (!fn) throw new Error('Target TypeScript function not found.');");
			sb.AppendLine();
			sb.AppendLine("    const arg = _parseTsArg(tc.input);");
			sb.AppendLine("    const res = fn(arg);");
			sb.AppendLine("    const actual = res !== null && res !== undefined ? String(res) : 'null';");
			sb.AppendLine("    const ok = actual.trim().toLowerCase() === String(tc.expected).trim().toLowerCase();");
			sb.AppendLine("    if (ok) passed++;");
			sb.AppendLine("    console.log(`[TC_RESULT:${idx}|PASSED:${ok}|ACTUAL:${actual}|HIDDEN:${tc.isHidden}]`);");
			sb.AppendLine("  } catch (err: any) {");
			sb.AppendLine("    console.log(`[TC_RESULT:${idx}|PASSED:false|ACTUAL:Runtime Error: ${err.message}|HIDDEN:${tc.isHidden}]`);");
			sb.AppendLine("  }");
			sb.AppendLine("});");
			sb.AppendLine("console.log(`[TC_SUMMARY:PASSED=${passed}|TOTAL=${testCases.length}]`);");
			return sb.ToString();
		}

		private static string EscapeTsString(string? input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			return input.Replace("\\", "\\\\").Replace("'", "\\'");
		}
	}
}

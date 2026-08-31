using Application.DTOs.Assessments;
using Application.Interfaces.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Implementation.Services.CodeHarnesses
{
	public class CSharpHarnessBuilder : ICodeHarnessBuilder
	{
		public int CompilationLineOffset => 6;

		public bool Supports(string language)
		{
			if (string.IsNullOrWhiteSpace(language)) return false;
			var lang = language.Trim().ToLower();
			return lang is "csharp" or "cs" or "c#";
		}

		public string BuildHarness(string userCode, string functionName, List<TestCaseItem> testCases)
		{
			var sb = new StringBuilder();
			sb.AppendLine("using System;");
			sb.AppendLine("using System.Collections.Generic;");
			sb.AppendLine("using System.Linq;");
			sb.AppendLine("using System.Reflection;");
			sb.AppendLine();

			// Strip duplicate namespace usings from userCode to prevent CS0105 warnings
			var cleanedUserCode = Regex.Replace(userCode, @"^\s*using\s+(System|System\.Collections\.Generic|System\.Linq|System\.Reflection)\s*;\s*\r?\n?", "", RegexOptions.Multiline);
			sb.AppendLine(cleanedUserCode);
			sb.AppendLine();
			sb.AppendLine("public class Program");
			sb.AppendLine("{");
			sb.AppendLine("    private static object ParseSingleArg(Type pType, string raw)");
			sb.AppendLine("    {");
			sb.AppendLine("        if (string.IsNullOrEmpty(raw)) return raw;");
			sb.AppendLine("        raw = raw.Trim();");
			sb.AppendLine("        if (pType == typeof(int)) return int.Parse(raw);");
			sb.AppendLine("        if (pType == typeof(long)) return long.Parse(raw);");
			sb.AppendLine("        if (pType == typeof(double)) return double.Parse(raw);");
			sb.AppendLine("        if (pType == typeof(bool)) return bool.Parse(raw);");
			sb.AppendLine("        if (pType == typeof(string)) return raw.Trim('\"');");
			sb.AppendLine("        if (pType == typeof(int[]))");
			sb.AppendLine("        {");
			sb.AppendLine("            var items = raw.Trim('[', ']').Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);");
			sb.AppendLine("            return items.Select(s => int.Parse(s.Trim())).ToArray();");
			sb.AppendLine("        }");
			sb.AppendLine("        if (pType == typeof(string[]))");
			sb.AppendLine("        {");
			sb.AppendLine("            var items = raw.Trim('[', ']').Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);");
			sb.AppendLine("            return items.Select(s => s.Trim().Trim('\"')).ToArray();");
			sb.AppendLine("        }");
			sb.AppendLine("        return raw;");
			sb.AppendLine("    }");
			sb.AppendLine();
			sb.AppendLine("    public static void Main()");
			sb.AppendLine("    {");
			sb.AppendLine("        var testCases = new (string Input, string Expected, bool IsHidden)[]");
			sb.AppendLine("        {");

			for (int i = 0; i < testCases.Count; i++)
			{
				var tc = testCases[i];
				var escapedInput = SymbolEscape(tc.Input);
				var escapedExpected = SymbolEscape(tc.ExpectedOutput);
				var hiddenStr = tc.IsHidden ? "true" : "false";
				sb.AppendLine($"            (@\"{escapedInput}\", @\"{escapedExpected}\", {hiddenStr}),");
			}

			sb.AppendLine("        };");
			sb.AppendLine();
			sb.AppendLine("        int passedCount = 0;");
			sb.AppendLine("        int idx = 0;");
			sb.AppendLine("        foreach (var tc in testCases)");
			sb.AppendLine("        {");
			sb.AppendLine("            try");
			sb.AppendLine("            {");
			var safeFnName = SymbolEscape(functionName ?? "Solve");
			sb.AppendLine("                var solutionType = typeof(Solution);");
			sb.AppendLine("                var allMethods = solutionType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)");
			sb.AppendLine("                    .Where(m => m.DeclaringType == typeof(Solution))");
			sb.AppendLine("                    .ToList();");
			sb.AppendLine($"                var method = allMethods.FirstOrDefault(m => string.Equals(m.Name, \"{safeFnName}\", StringComparison.OrdinalIgnoreCase))");
			sb.AppendLine("                    ?? allMethods.FirstOrDefault();");
			sb.AppendLine("                if (method == null) throw new Exception(\"Target solution method not found in Solution class.\");");
			sb.AppendLine();
			sb.AppendLine("                var parameters = method.GetParameters();");
			sb.AppendLine("                object[] args = new object[parameters.Length];");
			sb.AppendLine("                var inputVal = tc.Input.Trim();");
			sb.AppendLine("                if (parameters.Length == 1)");
			sb.AppendLine("                {");
			sb.AppendLine("                    args[0] = ParseSingleArg(parameters[0].ParameterType, inputVal);");
			sb.AppendLine("                }");
			sb.AppendLine("                else if (parameters.Length > 1)");
			sb.AppendLine("                {");
			sb.AppendLine("                    var parts = inputVal.Split(new char[] { ';', '|', ',' }, StringSplitOptions.RemoveEmptyEntries);");
			sb.AppendLine("                    for (int p = 0; p < parameters.Length && p < parts.Length; p++)");
			sb.AppendLine("                    {");
			sb.AppendLine("                        args[p] = ParseSingleArg(parameters[p].ParameterType, parts[p]);");
			sb.AppendLine("                    }");
			sb.AppendLine("                }");
			sb.AppendLine();
			sb.AppendLine("                object instance = method.IsStatic ? null : Activator.CreateInstance(solutionType);");
			sb.AppendLine("                object actualResult = method.Invoke(instance, args);");
			sb.AppendLine("                string actualStr = actualResult != null ? actualResult.ToString() : \"null\";");
			sb.AppendLine("                bool isMatch = string.Equals(actualStr.Trim(), tc.Expected.Trim(), StringComparison.OrdinalIgnoreCase);");
			sb.AppendLine("                if (isMatch) passedCount++;");
			sb.AppendLine();
			sb.AppendLine("                Console.WriteLine($\"[TC_RESULT:{idx}|PASSED:{isMatch}|ACTUAL:{actualStr}|HIDDEN:{tc.IsHidden}]\");");
			sb.AppendLine("            }");
			sb.AppendLine("            catch (Exception ex)");
			sb.AppendLine("            {");
			sb.AppendLine("                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;");
			sb.AppendLine("                Console.WriteLine($\"[TC_RESULT:{idx}|PASSED:False|ACTUAL:Runtime Error: {msg}|HIDDEN:{tc.IsHidden}]\");");
			sb.AppendLine("            }");
			sb.AppendLine("            idx++;");
			sb.AppendLine("        }");
			sb.AppendLine("        Console.WriteLine($\"[TC_SUMMARY:PASSED={passedCount}|TOTAL={testCases.Length}]\");");
			sb.AppendLine("    }");
			sb.AppendLine("}");

			return sb.ToString();
		}

		private static string SymbolEscape(string? input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			return input.Replace("\"", "\"\"");
		}
	}
}

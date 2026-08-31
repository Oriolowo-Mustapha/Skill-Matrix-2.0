using Application.DTOs.Assessments;
using Application.Interfaces.Service;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Implementation.Services.CodeHarnesses
{
	public class JavaHarnessBuilder : ICodeHarnessBuilder
	{
		public int CompilationLineOffset => 8;

		public bool Supports(string language)
		{
			if (string.IsNullOrWhiteSpace(language)) return false;
			var lang = language.Trim().ToLower();
			return lang is "java" or "openjdk";
		}

		public string BuildHarness(string userCode, string functionName, List<TestCaseItem> testCases)
		{
			var sb = new StringBuilder();
			sb.AppendLine("import java.util.*;");
			sb.AppendLine("import java.lang.reflect.*;");
			sb.AppendLine();
			// User's Solution class (if public, we make it package-private so it compiles alongside public class Main)
			var cleanedUserCode = userCode.Replace("public class Solution", "class Solution");
			sb.AppendLine(cleanedUserCode);
			sb.AppendLine();
			sb.AppendLine("public class Main {");
			sb.AppendLine("    private static Object parseSingleArg(Class<?> pType, String raw) {");
			sb.AppendLine("        if (raw == null || raw.isEmpty()) return raw;");
			sb.AppendLine("        raw = raw.trim();");
			sb.AppendLine("        if (pType == int.class || pType == Integer.class) return Integer.parseInt(raw);");
			sb.AppendLine("        if (pType == long.class || pType == Long.class) return Long.parseLong(raw);");
			sb.AppendLine("        if (pType == double.class || pType == Double.class) return Double.parseDouble(raw);");
			sb.AppendLine("        if (pType == boolean.class || pType == Boolean.class) return Boolean.parseBoolean(raw);");
			sb.AppendLine("        if (pType == String.class) return raw.replaceAll(\"^\\\"|\\\"$\", \"\");");
			sb.AppendLine("        if (pType == int[].class) {");
			sb.AppendLine("            String cleaned = raw.replaceAll(\"\\\\[|\\\\]\", \"\").trim();");
			sb.AppendLine("            if (cleaned.isEmpty()) return new int[0];");
			sb.AppendLine("            String[] parts = cleaned.split(\",\");");
			sb.AppendLine("            int[] arr = new int[parts.length];");
			sb.AppendLine("            for (int i = 0; i < parts.length; i++) arr[i] = Integer.parseInt(parts[i].trim());");
			sb.AppendLine("            return arr;");
			sb.AppendLine("        }");
			sb.AppendLine("        return raw;");
			sb.AppendLine("    }");
			sb.AppendLine();
			sb.AppendLine("    public static void main(String[] args) {");
			sb.AppendLine("        String[][] testCases = {");
			foreach (var tc in testCases)
			{
				sb.AppendLine($"            {{\"{EscapeJavaString(tc.Input)}\", \"{EscapeJavaString(tc.ExpectedOutput)}\", \"{(tc.IsHidden ? "true" : "false")}\"}},");
			}
			sb.AppendLine("        };");
			sb.AppendLine("        int passedCount = 0;");
			sb.AppendLine("        for (int idx = 0; idx < testCases.length; idx++) {");
			sb.AppendLine("            String inp = testCases[idx][0];");
			sb.AppendLine("            String exp = testCases[idx][1];");
			sb.AppendLine("            String isHidden = testCases[idx][2];");
			sb.AppendLine("            try {");
			var safeFn = string.IsNullOrWhiteSpace(functionName) ? "solve" : functionName.Trim();
			sb.AppendLine("                Class<?> solutionClass = Class.forName(\"Solution\");");
			sb.AppendLine("                Method targetMethod = null;");
			sb.AppendLine("                for (Method m : solutionClass.getDeclaredMethods()) {");
			sb.AppendLine($"                    if (m.getName().equalsIgnoreCase(\"{safeFn}\") || targetMethod == null) {{ targetMethod = m; if (m.getName().equalsIgnoreCase(\"{safeFn}\")) break; }}");
			sb.AppendLine("                }");
			sb.AppendLine("                if (targetMethod == null) throw new Exception(\"Target solution method not found.\");");
			sb.AppendLine("                targetMethod.setAccessible(true);");
			sb.AppendLine("                Class<?>[] paramTypes = targetMethod.getParameterTypes();");
			sb.AppendLine("                Object[] callArgs = new Object[paramTypes.length];");
			sb.AppendLine("                if (paramTypes.length == 1) {");
			sb.AppendLine("                    callArgs[0] = parseSingleArg(paramTypes[0], inp);");
			sb.AppendLine("                }");
			sb.AppendLine("                Object instance = Modifier.isStatic(targetMethod.getModifiers()) ? null : solutionClass.getDeclaredConstructor().newInstance();");
			sb.AppendLine("                Object result = targetMethod.invoke(instance, callArgs);");
			sb.AppendLine("                String actual = result != null ? String.valueOf(result) : \"null\";");
			sb.AppendLine("                boolean isMatch = actual.trim().equalsIgnoreCase(exp.trim());");
			sb.AppendLine("                if (isMatch) passedCount++;");
			sb.AppendLine("                System.out.println(\"[TC_RESULT:\" + idx + \"|PASSED:\" + (isMatch ? \"True\" : \"False\") + \"|ACTUAL:\" + actual + \"|HIDDEN:\" + (Boolean.parseBoolean(isHidden) ? \"True\" : \"False\") + \"]\");");
			sb.AppendLine("            } catch (Exception ex) {");
			sb.AppendLine("                Throwable cause = ex.getCause() != null ? ex.getCause() : ex;");
			sb.AppendLine("                System.out.println(\"[TC_RESULT:\" + idx + \"|PASSED:False|ACTUAL:Runtime Error: \" + cause.getMessage() + \"|HIDDEN:\" + (Boolean.parseBoolean(isHidden) ? \"True\" : \"False\") + \"]\");");
			sb.AppendLine("            }");
			sb.AppendLine("        }");
			sb.AppendLine("        System.out.println(\"[TC_SUMMARY:PASSED=\" + passedCount + \"|TOTAL=\" + testCases.length + \"]\");");
			sb.AppendLine("    }");
			sb.AppendLine("}");
			return sb.ToString();
		}

		private static string EscapeJavaString(string? input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
		}
	}
}

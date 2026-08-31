using Application.DTOs.Assessments;
using Application.Interfaces.Service;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrastructure.Implementation.Services
{
	public class CodeExecutionService : ICodeExecutionService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly ICodeHarnessFactory _harnessFactory;

		// Standard Judge0 CE language IDs (compatible with local Docker & RapidAPI)
		private static readonly Dictionary<string, int> LanguageMapping = new(StringComparer.OrdinalIgnoreCase)
		{
			{ "python", 71 },      // Python 3.8+
			{ "py", 71 },
			{ "javascript", 63 },  // Node.js
			{ "js", 63 },
			{ "csharp", 51 },      // C# (.NET Core / Mono)
			{ "cs", 51 },
			{ "java", 62 },        // Java OpenJDK
			{ "typescript", 74 },  // TypeScript
			{ "ts", 74 },
			{ "cpp", 54 },         // C++ GCC
			{ "c", 50 }            // C GCC
		};

		public CodeExecutionService(HttpClient httpClient, IConfiguration configuration, ICodeHarnessFactory harnessFactory)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_harnessFactory = harnessFactory;
		}

		public async Task<CodeExecutionResponseDTO> ExecuteCodeAsync(CodeExecutionRequestDTO request)
		{
			if (request == null || string.IsNullOrWhiteSpace(request.SourceCode))
			{
				return new CodeExecutionResponseDTO
				{
					IsSuccess = false,
					ConsoleOutput = string.Empty,
					ErrorMessage = "No source code provided. Please write a valid solution."
				};
			}

			try
			{
				var judgeConfig = _configuration.GetSection("Judge0");
				var baseUrl = judgeConfig["BaseUrl"] ?? "http://localhost:2358";
				var rapidApiKey = judgeConfig["RapidApiKey"];
				var rapidApiHost = judgeConfig["RapidApiHost"] ?? "judge0-ce.p.rapidapi.com";
				var apiKey = judgeConfig["ApiKey"];

				// Determine language ID
				var langKey = request.Language ?? "csharp";
				if (!LanguageMapping.TryGetValue(langKey, out int languageId))
				{
					languageId = 51; // Default to C#
				}

				// Build test harness code or synthesize fallback test case if needed
				var effectiveTestCases = (request.TestCases != null && request.TestCases.Any())
					? request.TestCases
					: new List<TestCaseItem>
					{
						new TestCaseItem
						{
							Input = request.SampleInput ?? request.ExpectedOutput ?? string.Empty,
							ExpectedOutput = request.ExpectedOutput ?? string.Empty,
							IsHidden = false
						}
					};

				bool isUserMainMethod = request.SourceCode.Contains("static void Main") || request.SourceCode.Contains("def main()") || request.SourceCode.Contains("public static void main");
				bool isHarnessApplied = !isUserMainMethod && _harnessFactory.HasHarness(langKey);

				int lineOffset = 0;
				string sourceToExecute = isHarnessApplied
					? _harnessFactory.BuildHarness(langKey, request.SourceCode, request.FunctionName ?? "Solve", effectiveTestCases, out lineOffset)
					: request.SourceCode;

				Console.WriteLine($"[Judge0 ExecuteCodeAsync] Lang={langKey}, Function={request.FunctionName}, TestCasesCount={effectiveTestCases.Count}, IsHarnessApplied={isHarnessApplied}, LineOffset={lineOffset}");
				Console.WriteLine("=== GENERATED SOURCE SENT TO JUDGE0 ===");
				Console.WriteLine(sourceToExecute);
				Console.WriteLine("========================================");

				var payloadObj = new
				{
					source_code = SafeBase64Encode(sourceToExecute),
					language_id = languageId,
					expected_output = (string?)null
				};

				var jsonString = JsonSerializer.Serialize(payloadObj);

				// Step 1: POST submission to Judge0
				var requestUri = $"{baseUrl.TrimEnd('/')}/submissions?base64_encoded=true";
				var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
				{
					Content = new StringContent(jsonString, Encoding.UTF8, "application/json")
				};

				AddAuthHeaders(requestMessage, rapidApiKey, rapidApiHost, apiKey);

				var response = await _httpClient.SendAsync(requestMessage);
				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					return FailResponse($"Judge0 sandbox API unavailable (HTTP {response.StatusCode}): {errorContent}");
				}

				// Step 2: Read submission token
				using var responseStream = await response.Content.ReadAsStreamAsync();
				var tokenResult = await JsonSerializer.DeserializeAsync<Judge0TokenResponse>(responseStream);
				if (string.IsNullOrEmpty(tokenResult?.Token))
				{
					return FailResponse("Failed to retrieve execution token from Judge0 sandbox.");
				}

				// Step 3: Poll submission status
				var token = tokenResult.Token;
				Judge0SubmissionResult? result = null;
				int attempts = 0;
				int maxAttempts = 20; // 8 seconds max window

				while (attempts < maxAttempts)
				{
					attempts++;
					await Task.Delay(400);

					var pollUri = $"{baseUrl.TrimEnd('/')}/submissions/{token}?base64_encoded=true";
					var pollRequest = new HttpRequestMessage(HttpMethod.Get, pollUri);
					AddAuthHeaders(pollRequest, rapidApiKey, rapidApiHost, apiKey);

					var pollResponse = await _httpClient.SendAsync(pollRequest);
					if (pollResponse.IsSuccessStatusCode)
					{
						using var pollStream = await pollResponse.Content.ReadAsStreamAsync();
						result = await JsonSerializer.DeserializeAsync<Judge0SubmissionResult>(pollStream);

						var currentStatusId = result?.Status?.Id ?? -1;
						if (currentStatusId >= 3) break; // 3+ = Finished (Accepted, Wrong Answer, Compile Error, etc.)
					}
				}

				if (result == null)
				{
					return FailResponse("Code execution timed out waiting for sandbox result.");
				}

				var stdout = SafeBase64Decode(result.Stdout);
				var stderr = SafeBase64Decode(result.Stderr);
				var compileOutput = SafeBase64Decode(result.CompileOutput);
				var message = SafeBase64Decode(result.Message);

				var statusId = result.Status?.Id ?? -1;
				var statusDescription = result.Status?.Description ?? "Execution error";

				// Compile error / Runtime error handling
				if (statusId == 6) // Compilation Error
				{
					return new CodeExecutionResponseDTO
					{
						IsSuccess = false,
						ConsoleOutput = stdout,
						ErrorMessage = $"Compilation Error:\n{compileOutput}",
						GeneratedSource = sourceToExecute,
						IsHarnessApplied = isHarnessApplied,
						CompilationLineOffset = lineOffset
					};
				}

				if (statusId != 3 && statusId != 4) // Not Accepted & Not Wrong Answer (e.g. Runtime Error, Time Limit)
				{
					var errDetail = !string.IsNullOrEmpty(compileOutput) ? compileOutput : (!string.IsNullOrEmpty(stderr) ? stderr : (!string.IsNullOrEmpty(message) ? message : statusDescription));
					return new CodeExecutionResponseDTO
					{
						IsSuccess = false,
						ConsoleOutput = stdout,
						ErrorMessage = $"Execution Status: {statusDescription}.\n{errDetail}",
						GeneratedSource = sourceToExecute,
						IsHarnessApplied = isHarnessApplied,
						CompilationLineOffset = lineOffset
					};
				}

				// If we ran with Test Harness, parse the structured test case results from stdout
				if (isHarnessApplied)
				{
					var harnessResult = ParseHarnessOutput(stdout, stderr, effectiveTestCases);
					harnessResult.GeneratedSource = sourceToExecute;
					harnessResult.IsHarnessApplied = isHarnessApplied;
					harnessResult.CompilationLineOffset = lineOffset;
					return harnessResult;
				}

				// Legacy single-output comparison
				var actualOutput = (stdout ?? string.Empty).Trim();
				var expectedOutput = (request.ExpectedOutput ?? string.Empty).Trim();
				var hasExpected = !string.IsNullOrEmpty(expectedOutput);
				var isMatch = !hasExpected || string.Equals(actualOutput, expectedOutput, StringComparison.OrdinalIgnoreCase);

				return new CodeExecutionResponseDTO
				{
					IsSuccess = isMatch,
					ConsoleOutput = stdout,
					ErrorMessage = isMatch ? string.Empty : $"Test Output Mismatch.\nExpected: '{expectedOutput}'\nActual: '{actualOutput}'",
					PassedCount = isMatch ? 1 : 0,
					TotalCount = 1,
					GeneratedSource = sourceToExecute,
					IsHarnessApplied = isHarnessApplied,
					CompilationLineOffset = 0
				};
			}
			catch (Exception ex)
			{
				return FailResponse($"Code Execution Exception: {ex.Message}");
			}
		}

		private CodeExecutionResponseDTO ParseHarnessOutput(string? stdout, string? stderr, List<TestCaseItem> testCases)
		{
			var testResults = new List<TestCaseResult>();
			int passedCount = 0;
			int totalCount = testCases.Count;

			if (!string.IsNullOrEmpty(stdout))
			{
				var lines = stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				var tcRegex = new Regex(@"\[TC_RESULT:(\d+)\|PASSED:(True|False|true|false)\|ACTUAL:(.*?)\|HIDDEN:(True|False|true|false)\]");
				var summaryRegex = new Regex(@"\[TC_SUMMARY:PASSED=(\d+)\|TOTAL=(\d+)\]");

				foreach (var line in lines)
				{
					var match = tcRegex.Match(line);
					if (match.Success)
					{
						int idx = int.Parse(match.Groups[1].Value);
						bool passed = bool.Parse(match.Groups[2].Value);
						string actual = match.Groups[3].Value;
						bool hidden = bool.Parse(match.Groups[4].Value);

						var originalTc = idx < testCases.Count ? testCases[idx] : null;

						testResults.Add(new TestCaseResult
						{
							TestCaseIndex = idx,
							Input = hidden ? null : originalTc?.Input,
							ExpectedOutput = hidden ? null : originalTc?.ExpectedOutput,
							ActualOutput = actual,
							Passed = passed,
							IsHidden = hidden
						});
					}

					var summaryMatch = summaryRegex.Match(line);
					if (summaryMatch.Success)
					{
						passedCount = int.Parse(summaryMatch.Groups[1].Value);
						totalCount = int.Parse(summaryMatch.Groups[2].Value);
					}
				}
			}

			// Clean console output (remove harness internal tags for user view)
			var cleanedOutput = Regex.Replace(stdout ?? string.Empty, @"\[TC_RESULT:.*?\]\r?\n?", string.Empty);
			cleanedOutput = Regex.Replace(cleanedOutput, @"\[TC_SUMMARY:.*?\]\r?\n?", string.Empty).Trim();

			bool isAllPassed = passedCount == totalCount && totalCount > 0;

			return new CodeExecutionResponseDTO
			{
				IsSuccess = isAllPassed,
				ConsoleOutput = !string.IsNullOrEmpty(cleanedOutput) ? cleanedOutput : (isAllPassed ? "All test cases executed cleanly." : "Execution complete with failing test cases."),
				ErrorMessage = isAllPassed ? string.Empty : $"{passedCount}/{totalCount} Test Cases Passed.",
				PassedCount = passedCount,
				TotalCount = totalCount,
				TestResults = testResults
			};
		}

		private CodeExecutionResponseDTO FailResponse(string errorMsg)
		{
			return new CodeExecutionResponseDTO
			{
				IsSuccess = false,
				ConsoleOutput = string.Empty,
				ErrorMessage = errorMsg,
				PassedCount = 0,
				TotalCount = 0
			};
		}

		private void AddAuthHeaders(HttpRequestMessage requestMessage, string? rapidApiKey, string? rapidApiHost, string? apiKey)
		{
			if (!string.IsNullOrEmpty(rapidApiKey))
			{
				requestMessage.Headers.Add("x-rapidapi-key", rapidApiKey);
				requestMessage.Headers.Add("x-rapidapi-host", rapidApiHost);
			}
			else if (!string.IsNullOrEmpty(apiKey))
			{
				requestMessage.Headers.Add("X-Auth-Token", apiKey);
			}
		}

		private string SafeBase64Encode(string? text)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
		}

		private string SafeBase64Decode(string? base64Str)
		{
			if (string.IsNullOrEmpty(base64Str)) return string.Empty;
			try
			{
				return Encoding.UTF8.GetString(Convert.FromBase64String(base64Str));
			}
			catch
			{
				return base64Str;
			}
		}

		private class Judge0TokenResponse
		{
			[JsonPropertyName("token")]
			public string? Token { get; set; }
		}

		private class Judge0SubmissionResult
		{
			[JsonPropertyName("stdout")]
			public string? Stdout { get; set; }

			[JsonPropertyName("stderr")]
			public string? Stderr { get; set; }

			[JsonPropertyName("compile_output")]
			public string? CompileOutput { get; set; }

			[JsonPropertyName("message")]
			public string? Message { get; set; }

			[JsonPropertyName("status")]
			public Judge0Status? Status { get; set; }
		}

		private class Judge0Status
		{
			[JsonPropertyName("id")]
			public int Id { get; set; }

			[JsonPropertyName("description")]
			public string? Description { get; set; }
		}
	}
}

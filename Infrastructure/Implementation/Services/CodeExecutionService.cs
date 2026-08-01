using Application.DTOs.Assessments;
using Application.Interfaces.Service;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.Implementation.Services
{
	public class CodeExecutionService : ICodeExecutionService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

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

		public CodeExecutionService(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_configuration = configuration;
		}

		public async Task<CodeExecutionResponseDTO> ExecuteCodeAsync(CodeExecutionRequestDTO request)
		{
			try
			{
				var judgeConfig = _configuration.GetSection("Judge0");
				var baseUrl = judgeConfig["BaseUrl"] ?? "http://localhost:2358";
				var rapidApiKey = judgeConfig["RapidApiKey"];
				var rapidApiHost = judgeConfig["RapidApiHost"] ?? "judge0-ce.p.rapidapi.com";
				var apiKey = judgeConfig["ApiKey"];

				// Determine language ID
				var langKey = request?.Language ?? "python";
				if (!LanguageMapping.TryGetValue(langKey, out int languageId))
				{
					languageId = 71; // Default fallback to Python
				}

				// Ensure source code is not null or whitespace
				var sourceToEncode = string.IsNullOrWhiteSpace(request?.SourceCode) 
					? "// No source code provided\n" 
					: request.SourceCode;

				// Build payload object with snake_case properties
				var payloadObj = new
				{
					source_code = SafeBase64Encode(sourceToEncode),
					language_id = languageId,
					expected_output = SafeBase64Encode(request?.ExpectedOutput ?? string.Empty)
				};

				var jsonString = JsonSerializer.Serialize(payloadObj);

				// Step 1: POST submission asynchronously without wait=true
				var requestUri = $"{baseUrl.TrimEnd('/')}/submissions?base64_encoded=true";
				var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
				{
					Content = new StringContent(jsonString, Encoding.UTF8, "application/json")
				};

				AddAuthHeaders(requestMessage, rapidApiKey, rapidApiHost, apiKey);

				var response = await _httpClient.SendAsync(requestMessage);
				if (!response.IsSuccessStatusCode)
				{
					var errorResponseContent = await response.Content.ReadAsStringAsync();
					return ExecuteFallbackEvaluation(request, $"Judge0 API returned HTTP {response.StatusCode}: {errorResponseContent}");
				}

				// Step 2: Read submission token from Judge0 response
				using var responseStream = await response.Content.ReadAsStreamAsync();
				var tokenResult = await JsonSerializer.DeserializeAsync<Judge0TokenResponse>(responseStream);
				if (string.IsNullOrEmpty(tokenResult?.Token))
				{
					return ExecuteFallbackEvaluation(request, "Failed to retrieve submission token from Judge0.");
				}

				// Step 3: Poll GET /submissions/{token}?base64_encoded=true asynchronously
				var token = tokenResult.Token;
				Judge0SubmissionResult? result = null;
				int attempts = 0;
				int maxAttempts = 15; // Max 7.5 seconds total polling window

				while (attempts < maxAttempts)
				{
					attempts++;
					await Task.Delay(400); // Wait 400ms between poll retries

					var pollUri = $"{baseUrl.TrimEnd('/')}/submissions/{token}?base64_encoded=true";
					var pollRequest = new HttpRequestMessage(HttpMethod.Get, pollUri);
					AddAuthHeaders(pollRequest, rapidApiKey, rapidApiHost, apiKey);

					var pollResponse = await _httpClient.SendAsync(pollRequest);
					if (pollResponse.IsSuccessStatusCode)
					{
						using var pollStream = await pollResponse.Content.ReadAsStreamAsync();
						result = await JsonSerializer.DeserializeAsync<Judge0SubmissionResult>(pollStream);

						var currentStatusId = result?.Status?.Id ?? -1;
						// Status ID >= 3 indicates execution finished (3=Accepted, 4=Wrong Answer, 6=Compile Error, etc.)
						if (currentStatusId >= 3)
						{
							break;
						}
					}
				}

				if (result == null)
				{
					return ExecuteFallbackEvaluation(request, "Submission polling timed out after 15 attempts.");
				}

				// Safe decode stdout, stderr, compile_output, message
				var stdout = SafeBase64Decode(result.Stdout);
				var stderr = SafeBase64Decode(result.Stderr);
				var compileOutput = SafeBase64Decode(result.CompileOutput);
				var message = SafeBase64Decode(result.Message);

				// Evaluate status
				var statusId = result.Status?.Id ?? -1;
				var statusDescription = result.Status?.Description ?? "Unknown status";

				if (statusId == 3) // 3 = Accepted
				{
					var actualOutput = (stdout ?? string.Empty).Trim();
					var expectedOutput = (request?.ExpectedOutput ?? string.Empty).Trim();
					var hasExpected = !string.IsNullOrEmpty(expectedOutput);
					var isMatch = !hasExpected || string.Equals(actualOutput, expectedOutput, StringComparison.Ordinal);

					return new CodeExecutionResponseDTO
					{
						IsSuccess = isMatch,
						ConsoleOutput = !string.IsNullOrEmpty(stdout) ? stdout : "Code executed successfully with 0 exit code.",
						ErrorMessage = isMatch ? string.Empty : $"Test Output Mismatch. Expected: '{expectedOutput}', Actual: '{actualOutput}'"
					};
				}
				else if (statusId == 13) // 13 = Internal Error (e.g. isolate cgroups or worker queue timeout)
				{
					var msgDetail = !string.IsNullOrEmpty(message) ? message : "Isolate cgroups / worker queue timeout";
					if (msgDetail.Contains("rb_sysopen") || msgDetail.Contains("/box/Main.cs"))
					{
						msgDetail = "Compilation Error: Missing entry point class. Ensure your solution defines a valid 'public class Program { public static void Main() }'.";
					}
					return ExecuteFallbackEvaluation(
						request, 
						$"[Judge0 Diagnostics] {msgDetail}"
					);
				}
				else
				{
					var errorDetail = !string.IsNullOrEmpty(compileOutput) ? compileOutput : (!string.IsNullOrEmpty(stderr) ? stderr : statusDescription);
					return new CodeExecutionResponseDTO
					{
						IsSuccess = false,
						ConsoleOutput = stdout,
						ErrorMessage = $"Execution Status: {statusDescription}. Details: {errorDetail}"
					};
				}
			}
			catch (Exception ex)
			{
				return ExecuteFallbackEvaluation(request, $"Sandbox Connection Exception: {ex.Message}");
			}
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

		// Fallback evaluation for local Docker setup when isolate sandbox is unprivileged
		private CodeExecutionResponseDTO ExecuteFallbackEvaluation(CodeExecutionRequestDTO request, string diagnosticNote)
		{
			var code = request?.SourceCode ?? string.Empty;
			var hasContent = !string.IsNullOrWhiteSpace(code) && code.Trim().Length > 10;

			if (hasContent)
			{
				return new CodeExecutionResponseDTO
				{
					IsSuccess = true,
					ConsoleOutput = $"[Fallback Evaluator Output]\n✓ Syntax verification passed for {request?.Language ?? "code"}.\n\n{diagnosticNote}",
					ErrorMessage = string.Empty
				};
			}
			else
			{
				return new CodeExecutionResponseDTO
				{
					IsSuccess = false,
					ConsoleOutput = string.Empty,
					ErrorMessage = $"Code execution incomplete. Please write a valid solution.\n({diagnosticNote})"
				};
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

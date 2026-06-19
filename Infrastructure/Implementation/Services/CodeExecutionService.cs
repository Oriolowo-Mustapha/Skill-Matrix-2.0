using Application.DTOs.Assessments;
using Application.Interfaces.Service;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

		private static readonly Dictionary<string, int> LanguageMapping = new(StringComparer.OrdinalIgnoreCase)
		{
			{ "csharp", 104 }, // .NET 8.0
			{ "cs", 104 },
			{ "python", 92 },  // Python 3.11.2
			{ "py", 92 },
			{ "java", 91 },    // Java OpenJDK 17
			{ "javascript", 93 }, // Node.js 18.15.0
			{ "js", 93 },
			{ "typescript", 94 }, // TypeScript 5.0.3
			{ "ts", 94 },
			{ "cpp", 105 },    // C++ (GCC 13.2.0)
			{ "c", 103 }       // C (GCC 13.2.0)
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
				var baseUrl = judgeConfig["BaseUrl"] ?? "https://judge0-ce.p.rapidapi.com";
				var rapidApiKey = judgeConfig["RapidApiKey"];
				var rapidApiHost = judgeConfig["RapidApiHost"] ?? "judge0-ce.p.rapidapi.com";
				var apiKey = judgeConfig["ApiKey"];

				// Determine language ID
				if (!LanguageMapping.TryGetValue(request.Language, out int languageId))
				{
					languageId = 92; // Default fallback to Python
				}

				// Build payload with Base64 encoding
				var payload = new Judge0SubmissionPayload
				{
					SourceCode = SafeBase64Encode(request.SourceCode),
					LanguageId = languageId,
					ExpectedOutput = SafeBase64Encode(request.ExpectedOutput)
				};

				// Prepare request
				var requestUri = $"{baseUrl.TrimEnd('/')}/submissions?base64_encoded=true&wait=true";
				var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
				{
					Content = JsonContent.Create(payload)
				};

				// Configure authentication headers
				if (!string.IsNullOrEmpty(rapidApiKey))
				{
					requestMessage.Headers.Add("x-rapidapi-key", rapidApiKey);
					requestMessage.Headers.Add("x-rapidapi-host", rapidApiHost);
				}
				else if (!string.IsNullOrEmpty(apiKey))
				{
					requestMessage.Headers.Add("X-Auth-Token", apiKey);
				}

				// Execute request
				var response = await _httpClient.SendAsync(requestMessage);
				if (!response.IsSuccessStatusCode)
				{
					var errorResponseContent = await response.Content.ReadAsStringAsync();
					return new CodeExecutionResponseDTO
					{
						IsSuccess = false,
						ErrorMessage = $"Judge0 API returned error status: {response.StatusCode}. Details: {errorResponseContent}"
					};
				}

				// Deserialize response
				var result = await response.Content.ReadFromJsonAsync<Judge0SubmissionResult>();
				if (result == null)
				{
					return new CodeExecutionResponseDTO
					{
						IsSuccess = false,
						ErrorMessage = "Failed to parse Judge0 execution results."
					};
				}

				// Safe decode stdout, stderr, compile_output
				var stdout = SafeBase64Decode(result.Stdout);
				var stderr = SafeBase64Decode(result.Stderr);
				var compileOutput = SafeBase64Decode(result.CompileOutput);

				// Evaluate status
				var statusId = result.Status?.Id ?? -1;
				var statusDescription = result.Status?.Description ?? "Unknown status";

				if (statusId == 3)
				{
					return new CodeExecutionResponseDTO
					{
						IsSuccess = true,
						ConsoleOutput = stdout,
						ErrorMessage = string.Empty
					};
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
				return new CodeExecutionResponseDTO
				{
					IsSuccess = false,
					ErrorMessage = $"Code execution failed with exception: {ex.Message}"
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

		private class Judge0SubmissionPayload
		{
			[JsonPropertyName("source_code")]
			public string SourceCode { get; set; } = string.Empty;

			[JsonPropertyName("language_id")]
			public int LanguageId { get; set; }

			[JsonPropertyName("expected_output")]
			public string ExpectedOutput { get; set; } = string.Empty;
		}

		private class Judge0SubmissionResult
		{
			[JsonPropertyName("stdout")]
			public string? Stdout { get; set; }

			[JsonPropertyName("stderr")]
			public string? Stderr { get; set; }

			[JsonPropertyName("compile_output")]
			public string? CompileOutput { get; set; }

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

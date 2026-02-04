using Application.Interfaces.Service;
using Domain.Entities;
using Infrastructure.DTOs;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Infrastructure.ExternalServices
{
	public class GeminiAiService : IAiService
	{
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private readonly string _model;

		public GeminiAiService(HttpClient httpClient, IConfiguration config)
		{
			_httpClient = httpClient;
			_apiKey = config["Gemini:ApiKey"];
			_model = config["Gemini:Model"];
		}

		public async Task<IEnumerable<Assessment>> GenerateAssessmentQuestionsAsync(string skillName, string proficencyLevel, int count = 10)
		{
			var prompt = $@"
				You are an expert technical interviewer.
				Generate {count} multiple-choice questions for the skill '{skillName}' at the '{proficencyLevel}' proficiency level.
				
				CRITICAL INSTRUCTION: Return ONLY a valid JSON array. No markdown formatting (like ```json), no explanations.
				
				Target JSON Format:
				[
				  {{
					""questionText"": ""The actual question text"",
					""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
					""correctAnswer"": ""The exact text of the correct option""
				  }}
				]";

			var jsonResponse = await CallGeminiApi(prompt);

			try
			{
				var dtos = JsonSerializer.Deserialize<List<GeminiQuestionDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return dtos.Select(d => d.ToEntity());
			}
			catch (JsonException)
			{
				throw new Exception("Failed to parse AI response. Raw response: " + jsonResponse);
			}
		}

		public async Task<ImprovementPlan> GenerateImprovementPlanAsync(AssessmentResult result)
		{
			var prompt = $@"
				A user just took a '{result.Skill.Name}' test and scored {result.Score}%.
				They got {result.NoOfWrongAnswers} questions wrong.
				
				Generate a personalized improvement plan.
				CRITICAL: Return ONLY valid JSON.
				
				Target JSON Format:
				{{
				  ""summary"": ""2 sentences summarizing their performance"",
				  ""focusAreas"": ""bullet points of what they need to study"",
				  ""resources"": [ 
					{{ ""title"": ""Resource Title"", ""url"": ""https://example.com"", ""type"": ""Article"" }} 
				  ]
				}}";

			var jsonResponse = await CallGeminiApi(prompt);
			try
			{
				var dto = JsonSerializer.Deserialize<GeminiPlanDto>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return dto.ToEntity();
			}
			catch (JsonException)
			{
				throw new Exception("Failed to parse AI response. Raw response: " + jsonResponse);
			}
		}

		private async Task<string> CallGeminiApi(string prompt)
		{
			var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

			var payload = new
			{
				contents = new[] {
					new { parts = new[] { new { text = prompt } } }
				}
			};

			var jsonPayload = JsonSerializer.Serialize(payload);
			var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync(url, content);
			response.EnsureSuccessStatusCode();

			var responseString = await response.Content.ReadAsStringAsync();
			using var doc = JsonDocument.Parse(responseString);

			return doc.RootElement
				.GetProperty("candidates")[0]
				.GetProperty("content")
				.GetProperty("parts")[0]
				.GetProperty("text")
				.GetString();
		}
	}
}
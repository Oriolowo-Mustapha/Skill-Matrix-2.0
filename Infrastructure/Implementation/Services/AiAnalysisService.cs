using Application.DTOs;
using Application.Interfaces.Service;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Implementation.Services
{
	public class AiAnalysisService : IAiAnalysisService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public AiAnalysisService(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_configuration = configuration;
		}

		public async Task<string> GenerateImprovementPlanAsync(List<AssessmentResultDTO> assessmentResults, CareerPathDTO targetCareerPath)
		{
			// Construct a prompt based on the user's gaps
			var sb = new StringBuilder();
			sb.AppendLine($"Generate a customized, step-by-step learning improvement plan for a user aiming for the '{targetCareerPath.Title}' career path.");
			sb.AppendLine("Their current assessment results are as follows:");
			foreach (var result in assessmentResults)
			{
				sb.AppendLine($"- Skill: {result.SkillName}, Score: {result.Score}/{result.TotalQuestions}, Level: {result.ProficiencyLevel}");
			}
			sb.AppendLine("Please identify key weaknesses and recommend a structured 4-week study plan focusing on bridging the gap.");

			var prompt = sb.ToString();

			var apiKey = _configuration["Gemini:ApiKey"];
			if (string.IsNullOrEmpty(apiKey))
			{
				// Fallback mock if no API key is provided
				return $"[MOCK AI RESPONSE]\nBased on your goal to become a {targetCareerPath.Title}, you need to focus heavily on the areas where you scored poorly. We recommend dedicating Week 1 to reviewing foundational concepts, Week 2 to practical exercises, Week 3 to advanced topics, and Week 4 to project building. (Note: Provide a Gemini API Key in appsettings to get real AI plans).";
			}

			var requestPayload = new
			{
				contents = new[]
				{
					new
					{
						parts = new[]
						{
							new { text = prompt }
						}
					}
				}
			};

			var jsonPayload = JsonSerializer.Serialize(requestPayload);
			var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}", content);

			if (response.IsSuccessStatusCode)
			{
				var jsonResponse = await response.Content.ReadAsStringAsync();
				using var document = JsonDocument.Parse(jsonResponse);
				var text = document.RootElement
					.GetProperty("candidates")[0]
					.GetProperty("content")
					.GetProperty("parts")[0]
					.GetProperty("text")
					.GetString();

				return text ?? "Unable to generate plan.";
			}

			return "Failed to contact AI service.";
		}
	}
}

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
			_apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey is not configured.");
			_model = config["Gemini:Model"] ?? throw new InvalidOperationException("Gemini:Model is not configured.");
		}

		public async Task<IEnumerable<Assessment>> GenerateAssessmentQuestionsAsync(string skillName, string proficencyLevel, int mcqCount, int codingCount, bool requiresCoding)
		{
			string prompt;

			if (requiresCoding && codingCount > 0)
			{
				prompt = $@"
				You are an expert technical interviewer and coding instructor.
				Generate exactly {mcqCount + codingCount} assessment questions for the skill '{skillName}' at the '{proficencyLevel}' proficiency level.

				The FIRST {mcqCount} questions MUST be multiple-choice theory questions. Each must have exactly 4 options and one correct answer.
				The LAST {codingCount} questions MUST be practical coding challenges. For coding challenges:
				- The 'questionText' should describe a coding problem (e.g. 'Write a function that returns the sum of two numbers').
				- The 'correctAnswer' must be set to 'CODE_CHALLENGE'.
				- The 'options' array must be empty [].
				- The 'expectedOutput' must contain the exact console output the correct solution should produce.
				- The 'questionType' must be 'Coding'.
				- The 'concept' must be a short name of the specific programming subtopic/concept being tested (e.g. 'Memory Management', 'Async/Await', 'Generics', 'LINQ' for C#; 'State Management', 'Hooks', 'Components' for React).

				For multiple-choice questions:
				- The 'questionType' must be 'MultipleChoice'.
				- The 'expectedOutput' must be null.
				- The 'concept' must be a short name of the specific programming subtopic/concept being tested.

				CRITICAL INSTRUCTION: Return ONLY a valid JSON array. No markdown formatting (like ```json), no explanations.

				Target JSON Format:
				[
				  {{
					""questionText"": ""The actual question text"",
					""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
					""correctAnswer"": ""The exact text of the correct option"",
					""questionType"": ""MultipleChoice"",
					""expectedOutput"": null,
					""concept"": ""LINQ""
				  }},
				  {{
					""questionText"": ""Write a function that prints 'Hello World'"",
					""options"": [],
					""correctAnswer"": ""CODE_CHALLENGE"",
					""questionType"": ""Coding"",
					""expectedOutput"": ""Hello World"",
					""concept"": ""Console I/O""
				  }}
				]";
			}
			else
			{
				int totalCount = mcqCount + codingCount;
				prompt = $@"
				You are an expert interviewer and assessor.
				Generate exactly {totalCount} assessment questions for the skill '{skillName}' at the '{proficencyLevel}' proficiency level.

				The FIRST {mcqCount} questions must be knowledge-based multiple-choice questions.
				The LAST {codingCount} questions must be scenario-based multiple-choice questions that test real-world decision making and practical application.

				ALL questions must have exactly 4 options and one correct answer.

				For ALL questions:
				- The 'questionType' must be 'MultipleChoice'.
				- The 'expectedOutput' must be null.
				- The 'concept' must be a short name of the specific subtopic/concept being tested (e.g. 'Data Visualization', 'Data Cleaning', 'SQL Joins' for Data Analyst; or 'Conflict Resolution', 'Delegation', 'Active Listening' for Leadership).

				CRITICAL INSTRUCTION: Return ONLY a valid JSON array. No markdown formatting (like ```json), no explanations.

				Target JSON Format:
				[
				  {{
					""questionText"": ""The actual question text"",
					""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
					""correctAnswer"": ""The exact text of the correct option"",
					""questionType"": ""MultipleChoice"",
					""expectedOutput"": null,
					""concept"": ""Conflict Resolution""
				  }}
				]";
			}

			var jsonResponse = await CallGeminiApi(prompt);

			try
			{
				var dtos = JsonSerializer.Deserialize<List<GeminiQuestionDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				if (dtos == null)
				{
					throw new Exception("Failed to deserialize AI response into questions.");
				}
				return dtos.Select(d => d.ToEntity());
			}
			catch (JsonException)
			{
				throw new Exception("Failed to parse AI response. Raw response: " + jsonResponse);
			}
		}

		public async Task<bool> ClassifySkillRequiresCodingAsync(string skillName)
		{
			var prompt = $@"
				Classify the following skill: '{skillName}'.
				Does proficiency in this skill require the ability to write programming code or scripts?
				Consider skills like C#, Python, React.js, JavaScript, SQL as coding skills.
				Consider skills like Data Analysis, Project Management, Communication, Leadership, Agile Methodology as non-coding skills.
				
				CRITICAL: Respond with ONLY the word 'true' or 'false'. Nothing else.";

			var response = await CallGeminiApi(prompt);
			var cleaned = response.Trim().ToLowerInvariant();

			return cleaned.Contains("true");
		}

		public async Task<ImprovementPlan> GenerateImprovementPlanAsync(AssessmentResult result, List<SkillGap> gaps)
		{
			var gapsDescription = new StringBuilder();
			if (gaps != null && gaps.Any())
			{
				gapsDescription.AppendLine("Specifically, they struggled with the following subtopics/concepts:");
				foreach (var gap in gaps)
				{
					gapsDescription.AppendLine($"- Subtopic: '{gap.Concept}', Score: {gap.Score}%");
				}
				gapsDescription.AppendLine("Make sure the generated focus areas and recommended resources directly address these specific subtopic weaknesses.");
			}
			else
			{
				gapsDescription.AppendLine("They performed consistently across all tested subtopics. Focus the improvement plan on advanced topics and maintaining their proficiency.");
			}

			var prompt = $@"
				A user just took a '{result.Skill.Name}' test at the '{result.ProficiencyLevel}' proficiency level and scored {result.Score}%.
				They got {result.NoOfWrongAnswers} questions wrong.
				
				{gapsDescription}
				
				Generate a personalized improvement plan with structured learning tasks.
				- For each concept failed, provide exactly 1 or 2 specific study or practice tasks in the 'tasks' array.
				- For each task, map it to a recommended learning resource (Article, Video, Course, etc.) in the 'resources' array. Ensure 'resourceTitle' in the task matches the 'title' of the resource.
				
				CRITICAL: Return ONLY valid JSON. Do not wrap the JSON in markdown formatting.
				
				Target JSON Format:
				{{
				  ""summary"": ""2 sentences summarizing their performance and where they need to improve"",
				  ""focusAreas"": ""bullet points of what they need to study, focusing on their specific gaps"",
				  ""tasks"": [
					{{ ""concept"": ""LINQ"", ""description"": ""Practice writing basic LINQ queries on arrays"", ""resourceTitle"": ""Introduction to LINQ"" }}
				  ],
				  ""resources"": [ 
					{{ ""title"": ""Introduction to LINQ"", ""url"": ""https://example.com/linq"", ""type"": ""Article"", ""concept"": ""LINQ"" }} 
				  ]
				}}";

			var jsonResponse = await CallGeminiApi(prompt);
			try
			{
				// Clean the markdown json formatting if Gemini added it
				string cleanJson = jsonResponse;
				if (cleanJson.Contains("```json"))
				{
					cleanJson = cleanJson.Replace("```json", "");
					cleanJson = cleanJson.Replace("```", "");
				}
				cleanJson = cleanJson.Trim();

				var dto = JsonSerializer.Deserialize<GeminiPlanDto>(cleanJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				if (dto == null)
				{
					throw new Exception("Failed to deserialize AI response into improvement plan.");
				}
				return dto.ToEntity();
			}
			catch (JsonException)
			{
				throw new Exception("Failed to parse AI response. Raw response: " + jsonResponse);
			}
		}

		public async Task<IEnumerable<Assessment>> GenerateTargetedQuestionsAsync(string skillName, string proficencyLevel, string concept, int count, bool requiresCoding)
		{
			string prompt;
			if (requiresCoding)
			{
				prompt = $@"
				You are an expert technical interviewer.
				Generate exactly {count} assessment questions for the skill '{skillName}' at the '{proficencyLevel}' proficiency level.
				CRITICAL: All questions must specifically test the subtopic/concept '{concept}'.

				The FIRST {count - 1} questions MUST be multiple-choice theory questions. Each must have exactly 4 options and one correct answer.
				The LAST 1 question MUST be a practical coding challenge. For the coding challenge:
				- The 'questionText' should describe a coding problem testing '{concept}'.
				- The 'correctAnswer' must be set to 'CODE_CHALLENGE'.
				- The 'options' array must be empty [].
				- The 'expectedOutput' must contain the exact console output the correct solution should produce.
				- The 'questionType' must be 'Coding'.
				- The 'concept' must be set to '{concept}'.

				For multiple-choice questions:
				- The 'questionType' must be 'MultipleChoice'.
				- The 'expectedOutput' must be null.
				- The 'concept' must be set to '{concept}'.

				CRITICAL INSTRUCTION: Return ONLY a valid JSON array. No markdown formatting (like ```json), no explanations.

				Target JSON Format:
				[
				  {{
					""questionText"": ""A multiple-choice question testing {concept}"",
					""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
					""correctAnswer"": ""The exact text of the correct option"",
					""questionType"": ""MultipleChoice"",
					""expectedOutput"": null,
					""concept"": ""{concept}""
				  }}
				]";
			}
			else
			{
				prompt = $@"
				You are an expert technical assessor.
				Generate exactly {count} multiple-choice assessment questions for the skill '{skillName}' at the '{proficencyLevel}' proficiency level.
				CRITICAL: All questions must specifically test the subtopic/concept '{concept}'.

				Each question must have exactly 4 options and one correct answer.
				For all questions:
				- The 'questionType' must be 'MultipleChoice'.
				- The 'expectedOutput' must be null.
				- The 'concept' must be set to '{concept}'.

				CRITICAL INSTRUCTION: Return ONLY a valid JSON array. No markdown formatting (like ```json), no explanations.

				Target JSON Format:
				[
				  {{
					""questionText"": ""A question testing {concept}"",
					""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
					""correctAnswer"": ""The exact text of the correct option"",
					""questionType"": ""MultipleChoice"",
					""expectedOutput"": null,
					""concept"": ""{concept}""
				  }}
				]";
			}

			var jsonResponse = await CallGeminiApi(prompt);
			try
			{
				var dtos = JsonSerializer.Deserialize<List<GeminiQuestionDto>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				if (dtos == null)
				{
					throw new Exception("Failed to deserialize AI response into targeted questions.");
				}
				return dtos.Select(d => d.ToEntity());
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
				.GetString() ?? throw new Exception("AI response text was null.");
		}
	}
}
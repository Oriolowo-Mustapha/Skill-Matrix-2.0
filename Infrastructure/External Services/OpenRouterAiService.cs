using Application.DTOs.Ai;
using Application.Interfaces.Service;
using Domain.Entities;
using Infrastructure.DTOs;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Infrastructure.ExternalServices
{
	public class OpenRouterAiService : IAiService
	{
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private readonly string _model;

		public OpenRouterAiService(HttpClient httpClient, IConfiguration config)
		{
			_httpClient = httpClient;
			_apiKey = config["OpenRouter:ApiKey"] ?? throw new InvalidOperationException("OpenRouter:ApiKey is not configured.");
			_model = config["OpenRouter:Model"] ?? "meta-llama/llama-3-8b-instruct:free";
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
				- The 'questionText' should describe a CodeSignal-style coding problem (e.g. 'Write a program that takes an integer N and prints the sum of even numbers from 1 to N').
				- The 'correctAnswer' must be set to 'CODE_CHALLENGE'.
				- The 'options' array must be empty [].
				- The 'sampleInput' must state the sample test input passed to the program (e.g. 'Input N = 10').
				- The 'expectedOutput' must contain the exact expected console output (e.g. '30').
				- The 'codeTemplate' must contain starter code with a valid entry point (e.g. for C#, 'using System; public class Program {{ public static void Main() {{ Console.WriteLine(30); }} }}').
				- The 'questionType' must be 'Coding'.
				- The 'concept' must be a short name of the specific programming subtopic/concept being tested.

				For multiple-choice questions:
				- The 'questionType' must be 'MultipleChoice'.
				- The 'sampleInput' must be null.
				- The 'expectedOutput' must be null.
				- The 'codeTemplate' must be null.
				- The 'concept' must be a short name of the specific programming subtopic/concept being tested.

				CRITICAL INSTRUCTION: Return ONLY a valid JSON array. No markdown formatting (like ```json), no explanations.

				Target JSON Format:
				[
				  {{
					""questionText"": ""The actual question text"",
					""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
					""correctAnswer"": ""The exact text of the correct option"",
					""questionType"": ""MultipleChoice"",
					""sampleInput"": null,
					""expectedOutput"": null,
					""codeTemplate"": null,
					""concept"": ""LINQ""
				  }},
				  {{
					""questionText"": ""Write a program that prints Hello World"",
					""options"": [],
					""correctAnswer"": ""CODE_CHALLENGE"",
					""questionType"": ""Coding"",
					""sampleInput"": ""Standard Execution"",
					""expectedOutput"": ""Hello World"",
					""codeTemplate"": ""using System; public class Program {{ public static void Main() {{ Console.WriteLine(""""Hello World""""); }} }}"",
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

			var jsonResponse = await CallOpenRouterApi(prompt);

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

			var response = await CallOpenRouterApi(prompt);
			var cleaned = response.Trim().ToLowerInvariant();

			return cleaned.Contains("true");
		}

		public async Task<ImprovementPlan> GenerateImprovementPlanAsync(AssessmentResult result, List<SkillGap> gaps)
		{
			var gapsDescription = new StringBuilder();
			if (gaps != null && gaps.Any())
			{
				gapsDescription.AppendLine("Specifically, the user struggled with the following subtopics/concepts:");
				foreach (var gap in gaps)
				{
					gapsDescription.AppendLine($"- Subtopic: '{gap.Concept}', Score: {gap.Score}%");
				}
				gapsDescription.AppendLine("Ensure that all generated focus areas, learning tasks, and recommended resources directly target these specific concept weaknesses.");
			}
			else
			{
				gapsDescription.AppendLine("The user performed consistently across all tested subtopics. Focus the improvement plan on advanced mastery and maintaining proficiency.");
			}

			var prompt = $@"
				You are an expert tech mentor and learning path designer.
				A user just completed a '{result.Skill.Name}' assessment at the '{result.ProficiencyLevel}' level with a score of {result.Score}%.
				Incorrect answers count: {result.NoOfWrongAnswers}.
				
				{gapsDescription}
				
				Generate a structured, highly actionable personalized improvement plan.
				- Provide a 2-sentence summary of their performance.
				- List key focus areas directly addressing their weak concepts.
				- For each weak concept, provide 1 or 2 specific, hands-on learning tasks in the 'tasks' array.
				- For each task, map it to a recommended high-quality learning resource (Official Docs, Microsoft Learn, MDN, Python Docs, freeCodeCamp, or W3Schools) in the 'resources' array.
				- IMPORTANT: Ensure 'resourceTitle' in each task EXACTLY matches the 'title' of its corresponding item in the 'resources' array.
				- Provide valid, real URLs for resources (e.g. 'https://learn.microsoft.com', 'https://developer.mozilla.org', 'https://docs.python.org/3/'). Do NOT use dummy example.com links.
				
				CRITICAL: Return ONLY valid JSON. Do NOT include markdown code fences (like ```json) or conversational explanations.
				
				Target JSON Format:
				{{
				  ""summary"": ""2 sentences summarizing their performance and key growth areas"",
				  ""focusAreas"": ""• Bullet point 1\n• Bullet point 2"",
				  ""tasks"": [
					{{ ""concept"": ""{gaps?.FirstOrDefault()?.Concept ?? "Core Concepts"}"", ""description"": ""Practice writing basic queries and error handlers"", ""resourceTitle"": ""Official Documentation Guide"" }}
				  ],
				  ""resources"": [ 
					{{ ""title"": ""Official Documentation Guide"", ""url"": ""https://learn.microsoft.com"", ""type"": ""Article"", ""concept"": ""{gaps?.FirstOrDefault()?.Concept ?? "Core Concepts"}"" }} 
				  ]
				}}";

			var jsonResponse = await CallOpenRouterApi(prompt);
			try
			{
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
				You are an expert technical interviewer and coding instructor.
				Generate exactly {count} assessment questions for the skill '{skillName}' at the '{proficencyLevel}' proficiency level.
				CRITICAL: All questions must specifically test the subtopic/concept '{concept}'.

				The FIRST {count - 1} questions MUST be multiple-choice theory questions. Each must have exactly 4 options and one correct answer.
				The LAST 1 question MUST be a practical coding challenge. For the coding challenge:
				- The 'questionText' should describe a CodeSignal-style coding problem testing '{concept}'.
				- The 'correctAnswer' must be set to 'CODE_CHALLENGE'.
				- The 'options' array must be empty [].
				- The 'sampleInput' must state the test input passed to the program (e.g. 'Input N = 10' or 'Standard Execution').
				- The 'expectedOutput' must contain the exact expected console output.
				- The 'codeTemplate' must contain starter boilerplate code with a valid entry point (e.g. for C#, 'using System; public class Program {{ public static void Main() {{ Console.WriteLine(30); }} }}').
				- The 'questionType' must be 'Coding'.
				- The 'concept' must be set to '{concept}'.

				For multiple-choice questions:
				- The 'questionType' must be 'MultipleChoice'.
				- The 'sampleInput' must be null.
				- The 'expectedOutput' must be null.
				- The 'codeTemplate' must be null.
				- The 'concept' must be set to '{concept}'.

				CRITICAL INSTRUCTION: Return ONLY a valid JSON array. No markdown formatting (like ```json), no explanations.

				Target JSON Format:
				[
				  {{
					""questionText"": ""A multiple-choice question testing {concept}"",
					""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
					""correctAnswer"": ""The exact text of the correct option"",
					""questionType"": ""MultipleChoice"",
					""sampleInput"": null,
					""expectedOutput"": null,
					""codeTemplate"": null,
					""concept"": ""{concept}""
				  }},
				  {{
					""questionText"": ""Write a program that tests {concept}"",
					""options"": [],
					""correctAnswer"": ""CODE_CHALLENGE"",
					""questionType"": ""Coding"",
					""sampleInput"": ""Standard Execution"",
					""expectedOutput"": ""Expected Output String"",
					""codeTemplate"": ""using System; public class Program {{ public static void Main() {{ Console.WriteLine(""""Expected Output String""""); }} }}"",
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
				- The 'sampleInput' must be null.
				- The 'expectedOutput' must be null.
				- The 'codeTemplate' must be null.
				- The 'concept' must be set to '{concept}'.

				CRITICAL INSTRUCTION: Return ONLY a valid JSON array. No markdown formatting (like ```json), no explanations.

				Target JSON Format:
				[
				  {{
					""questionText"": ""A question testing {concept}"",
					""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
					""correctAnswer"": ""The exact text of the correct option"",
					""questionType"": ""MultipleChoice"",
					""sampleInput"": null,
					""expectedOutput"": null,
					""codeTemplate"": null,
					""concept"": ""{concept}""
				  }}
				]";
			}

			var jsonResponse = await CallOpenRouterApi(prompt);
			try
			{
				string cleanJson = jsonResponse;
				if (cleanJson.Contains("```json"))
				{
					cleanJson = cleanJson.Replace("```json", "");
					cleanJson = cleanJson.Replace("```", "");
				}
				cleanJson = cleanJson.Trim();

				var dtos = JsonSerializer.Deserialize<List<GeminiQuestionDto>>(cleanJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

		public async Task<List<GeneratedTrackSkillDto>> GenerateSkillsForTrackAsync(string careerPathTitle, string trackName)
		{
			var prompt = $@"
				You are an expert career and technical advisor.
				A user is creating a new track called '{trackName}' under the career path '{careerPathTitle}'.
				Generate a comprehensive list of required skills for this track.
				For each skill, determine the minimum proficiency level required by the end of the track.
				
				Valid proficiency levels are EXACTLY one of these strings: ""Novice"", ""Begineer"", ""Intermediate"", ""Proficient"", ""Expert"". (Note the spelling of 'Begineer' MUST be used if you mean Beginner).

				CRITICAL INSTRUCTION: Return ONLY a valid JSON array. No markdown formatting (like ```json), no explanations.

				Target JSON Format:
				[
				  {{
					""skillName"": ""Skill Name (e.g. C#)"",
					""description"": ""Why this skill is needed for {trackName}"",
					""targetLevel"": ""Intermediate""
				  }}
				]";

			var jsonResponse = await CallOpenRouterApi(prompt);
			try
			{
				string cleanJson = jsonResponse;
				if (cleanJson.Contains("```json"))
				{
					cleanJson = cleanJson.Replace("```json", "");
					cleanJson = cleanJson.Replace("```", "");
				}
				cleanJson = cleanJson.Trim();

				var dtos = JsonSerializer.Deserialize<List<Application.DTOs.Ai.GeneratedTrackSkillDto>>(cleanJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				if (dtos == null)
				{
					throw new Exception("Failed to deserialize AI response into track skills.");
				}
				return dtos;
			}
			catch (JsonException)
			{
				throw new Exception("Failed to parse AI response. Raw response: " + jsonResponse);
			}
		}

		public async Task<List<Application.DTOs.Ai.AiCatalogPathDto>> GenerateGlobalCatalogAsync(List<string> existingSkillNames)
		{
			var skillsListStr = string.Join(", ", existingSkillNames);
			var prompt = $@"
				You are an expert Chief Technology Officer (CTO) and Enterprise Learning Architect.
				We have the following list of available skills in our system database:
				[{skillsListStr}]

				Your task is to analyze these skills and organize them into a comprehensive, professional set of Stack-Specialized Career Paths and Tracks.
				
				Rules:
				1. Create 4 to 6 Industry Career Paths:
				   - 'Software Engineering & Application Development'
				   - 'Frontend Web Development'
				   - 'Cloud Infrastructure & DevOps'
				   - 'Data Engineering & Artificial Intelligence'
				   - 'Mobile Application Development'
				   - 'Cybersecurity & Infrastructure'

				2. CRITICAL STACK-SPECIALIZATION RULE: Under each Career Path, DO NOT create generic single tracks. You MUST create stack-specific tracks based on tech ecosystems:
				   - Under 'Software Engineering': create separate specialized tracks such as '.NET & C# Enterprise Developer', 'Node.js & MERN Stack Developer', 'Python & Django/FastAPI Developer', 'Java & Spring Boot Developer'.
				   - Under 'Frontend Web Development': create specialized tracks like 'React & Next.js Modern Web Developer', 'Angular Enterprise Web Developer', 'Vue.js Web Developer'.
				   - Under 'Mobile Application Development': create specialized tracks like 'Flutter & Dart Mobile Developer', 'React Native Mobile Developer', 'Native iOS & Android Engineer'.
				   - Under 'Cloud Infrastructure & DevOps': create specialized tracks like 'AWS Cloud Solutions Engineer', 'Azure Enterprise Cloud Engineer', 'DevOps & SRE Engineer'.
				   - Under 'Data Engineering & Artificial Intelligence': create specialized tracks like 'Python Data Science & ML Engineer', 'Big Data & Data Pipeline Engineer'.

				3. Under each specialized track, populate it with relevant skills from the provided database list!
				4. For shared foundational skills (e.g. Git, REST API Design, Docker, Microservices, SQL), map them across multiple stack tracks where appropriate.
				5. For each selected skill in a track, assign a targetLevel integer between 0 and 4:
				   0 = Novice, 1 = Beginner, 2 = Intermediate, 3 = Proficient, 4 = Expert.
				6. Strictly prioritize using skill names from the provided database list!

				CRITICAL INSTRUCTION: Return ONLY a valid JSON array. Do NOT wrap in markdown code fences (like ```json). No conversational filler.

				Target JSON Format:
				[
				  {{
					""title"": ""Software Engineering & Application Development"",
					""description"": ""Specialized engineering pathways focused on modern backend stacks, APIs, and software architecture."",
					""tracks"": [
					  {{
						""name"": "".NET & C# Enterprise Developer"",
						""description"": ""Build scalable backend microservices, web APIs, and enterprise systems using C# and ASP.NET Core."",
						""skills"": [
						  {{ ""skillName"": ""C#"", ""targetLevel"": 4 }},
						  {{ ""skillName"": ""ASP.NET Core"", ""targetLevel"": 3 }},
						  {{ ""skillName"": ""SQL Server"", ""targetLevel"": 3 }}
						]
					  }},
					  {{
						""name"": ""Node.js & MERN Stack Developer"",
						""description"": ""Build full-stack JavaScript/TypeScript web applications and asynchronous RESTful services."",
						""skills"": [
						  {{ ""skillName"": ""JavaScript"", ""targetLevel"": 4 }},
						  {{ ""skillName"": ""Express.js"", ""targetLevel"": 3 }},
						  {{ ""skillName"": ""MongoDB"", ""targetLevel"": 3 }}
						]
					  }}
					]
				  }}
				]";

			var jsonResponse = await CallOpenRouterApi(prompt);
			try
			{
				string cleanJson = jsonResponse;
				if (cleanJson.Contains("```json"))
				{
					cleanJson = cleanJson.Replace("```json", "");
					cleanJson = cleanJson.Replace("```", "");
				}
				cleanJson = cleanJson.Trim();

				var dtos = JsonSerializer.Deserialize<List<Application.DTOs.Ai.AiCatalogPathDto>>(cleanJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return dtos ?? new List<Application.DTOs.Ai.AiCatalogPathDto>();
			}
			catch (JsonException)
			{
				throw new Exception("Failed to parse AI global catalog response. Raw response: " + jsonResponse);
			}
		}

		public async Task<List<Application.DTOs.Ai.AiSkillCatalogItemDto>> GenerateSkillCatalogAsync()
		{
			var prompt = @"You are a comprehensive technology skills expert. Generate a complete catalog of technology skills organized by categories.

For EACH of the following categories, generate at least 15 specific skills (including languages, frameworks, tools, libraries, platforms, and methodologies):

1. **Programming Languages** - All major languages (C#, Python, JavaScript, TypeScript, Java, C++, Go, Rust, PHP, Ruby, Swift, Kotlin, Dart, R, Scala, Elixir, Haskell, Perl, Lua, MATLAB, etc.)
2. **Frontend** - All major frontend frameworks, libraries, and tools (React, Angular, Vue.js, Next.js, Svelte, Nuxt.js, Blazor WebAssembly, jQuery, Tailwind CSS, Bootstrap, SASS, Webpack, Vite, Storybook, etc.)
3. **Backend** - All major backend frameworks (ASP.NET Core, Node.js, Express.js, Django, Flask, FastAPI, Spring Boot, Ruby on Rails, Laravel, NestJS, Koa.js, Gin, Fiber, Phoenix, etc.)
4. **Databases** - All major databases (PostgreSQL, MySQL, SQL Server, MongoDB, Redis, Elasticsearch, DynamoDB, Cassandra, Firebase, SQLite, Oracle, CouchDB, Neo4j, InfluxDB, etc.)
5. **Cloud** - All major cloud platforms and services (AWS, Azure, GCP, Heroku, DigitalOcean, Vercel, Netlify, Cloudflare, AWS Lambda, Azure Functions, S3, EC2, etc.)
6. **DevOps** - All major DevOps tools and practices (Docker, Kubernetes, Jenkins, GitHub Actions, GitLab CI, Terraform, Ansible, Helm, Prometheus, Grafana, Nginx, Apache, Linux Administration, etc.)
7. **AI/ML** - All major AI/ML frameworks and tools (TensorFlow, PyTorch, scikit-learn, Keras, OpenCV, Hugging Face, LangChain, GPT API, NLTK, SpaCy, MLflow, etc.)
8. **Cybersecurity** - All major security skills (Penetration Testing, OWASP, Network Security, Ethical Hacking, Encryption, Identity Management, SOC Analysis, Vulnerability Assessment, Compliance, etc.)
9. **Mobile** - All major mobile development technologies (React Native, Flutter, Swift/iOS, Kotlin/Android, Xamarin, MAUI, Ionic, Expo, etc.)
10. **Data Science** - All major data science tools (Pandas, NumPy, Matplotlib, Jupyter, Tableau, Power BI, Apache Spark, Hadoop, Airflow, dbt, Snowflake, etc.)
11. **Game Development** - All major game development tools (Unity, Unreal Engine, Godot, Phaser, Three.js, WebGL, Blender, etc.)
12. **Blockchain** - All major blockchain technologies (Solidity, Ethereum, Web3.js, Hyperledger, Smart Contracts, DeFi, NFT Development, etc.)
13. **Networking** - All major networking skills (TCP/IP, DNS, HTTP/HTTPS, Load Balancing, VPN, Firewall Configuration, Wireshark, SDN, etc.)
14. **Testing** - All major testing frameworks and tools (Jest, Selenium, Cypress, Playwright, xUnit, NUnit, JUnit, Mocha, Postman, k6, JMeter, etc.)
15. **Software Architecture** - All major architecture skills (Microservices, REST API Design, GraphQL, Event-Driven Architecture, Domain-Driven Design, CQRS, Design Patterns, System Design, Clean Architecture, etc.)

Return a JSON array of objects. Each object must have exactly two fields:
- ""name"": The skill name (clean, concise, no parenthetical suffixes)
- ""category"": The exact category name from the list above

Return ONLY the raw JSON array, no markdown formatting, no explanation. Example:
[
  {""name"": ""C#"", ""category"": ""Programming Languages""},
  {""name"": ""React"", ""category"": ""Frontend""}
]";

			var jsonResponse = await CallOpenRouterApi(prompt);
			try
			{
				string cleanJson = jsonResponse;
				if (cleanJson.Contains("```json"))
				{
					cleanJson = cleanJson.Replace("```json", "");
					cleanJson = cleanJson.Replace("```", "");
				}
				cleanJson = cleanJson.Trim();

				var skills = JsonSerializer.Deserialize<List<Application.DTOs.Ai.AiSkillCatalogItemDto>>(cleanJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				return skills ?? new List<Application.DTOs.Ai.AiSkillCatalogItemDto>();
			}
			catch (JsonException)
			{
				throw new Exception("Failed to parse AI skill catalog response. Raw response: " + jsonResponse);
			}
		}

		private async Task<string> CallOpenRouterApi(string prompt)
		{
			var url = "https://openrouter.ai/api/v1/chat/completions";

			var payload = new
			{
				model = _model,
				messages = new[]
				{
					new { role = "user", content = prompt }
				}
			};

			var jsonPayload = JsonSerializer.Serialize(payload);
			
			var request = new HttpRequestMessage(HttpMethod.Post, url);
			request.Headers.Add("Authorization", $"Bearer {_apiKey}");
			
			request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			var response = await _httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			var responseString = await response.Content.ReadAsStringAsync();
			using var doc = JsonDocument.Parse(responseString);

			return doc.RootElement
				.GetProperty("choices")[0]
				.GetProperty("message")
				.GetProperty("content")
				.GetString() ?? throw new Exception("AI response text was null.");
		}
	}
}

using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using MediatR;
using System.Text.Json;

namespace Application.Features.ImprovementPlans.Commands.GenerateAiImprovementPlan
{
	public class GenerateAiImprovementPlanCommandHandler : IRequestHandler<GenerateAiImprovementPlanCommand, BaseResponse<AIImprovementPlanResponseDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAiAnalysisService _aiAnalysisService;

		public GenerateAiImprovementPlanCommandHandler(IUnitOfWork unitOfWork, IAiAnalysisService aiAnalysisService)
		{
			_unitOfWork = unitOfWork;
			_aiAnalysisService = aiAnalysisService;
		}

		public async Task<BaseResponse<AIImprovementPlanResponseDTO>> Handle(GenerateAiImprovementPlanCommand request, CancellationToken cancellationToken)
		{
			var teamMember = await _unitOfWork.TeamMembers.GetByIdAsync(request.TeamMemberId);
			if (teamMember == null) throw new NotFoundException($"Team member with ID {request.TeamMemberId} not found.");

			var targetCareerPath = await _unitOfWork.CareerPaths.GetByIdAsync(request.TargetCareerPathId);
			if (targetCareerPath == null) throw new NotFoundException($"Career path not found.");

			// Fetch latest assessment results for the user (in a real scenario, filter for the best/latest score per skill)
			var allResults = await _unitOfWork.AssessmentResults.GetAllAsync();
			var userResults = allResults.Where(a => a.TeamMemberID == request.TeamMemberId).ToList();

			// Map to DTOs
			var resultDtos = userResults.Select(r => new AssessmentResultDTO
			{
				Id = r.Id,
				SkillName = r.Skill.Name,
				Score = r.Score,
				ProficiencyLevel = r.ProficiencyLevel.ToString(),
				TotalQuestions = r.TotalQuestions
			}).ToList();

			var careerPathDto = new CareerPathDTO
			{
				Id = targetCareerPath.Id,
				Title = targetCareerPath.Title,
				Description = targetCareerPath.Description
			};

			// Ask AI to generate the improvement plan text
			var aiResponseJsonText = await _aiAnalysisService.GenerateImprovementPlanAsync(resultDtos, careerPathDto);

			// We need to parse the AI JSON text back into AIImprovementPlanResponseDTO. 
			// Assuming the AI strictly returns JSON matching AIImprovementPlanResponseDTO.
			AIImprovementPlanResponseDTO generatedPlan;
			try
			{
				// Clean the markdown json formatting if Gemini added it
				string cleanJson = aiResponseJsonText;
				cleanJson = cleanJson.Replace("```json", "");
				cleanJson = cleanJson.Replace("```", "");
				cleanJson = cleanJson.Trim();
				generatedPlan = JsonSerializer.Deserialize<AIImprovementPlanResponseDTO>(cleanJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
					?? new AIImprovementPlanResponseDTO { OverallSummary = "AI returned empty plan." };
			}
			catch
			{
				// Fallback if parsing fails
				generatedPlan = new AIImprovementPlanResponseDTO
				{
					OverallSummary = aiResponseJsonText,
					FocusAreas = new List<string> { "Review generated text." },
					RecommendedCourses = new List<AIRecommendationDTO>()
				};
			}

			// Save to database as a new Improvement Plan for each gap. 
			// We can attach it to the latest Assessment Result. 
			if (userResults.Any())
			{
				var latestResult = userResults.OrderByDescending(r => r.DateCreated).First();
				var newPlan = new Domain.Entities.ImprovementPlan
				{
					AssessmentResultId = latestResult.Id,
					GeneratedSummary = generatedPlan.OverallSummary,
					FocusArea = string.Join(", ", generatedPlan.FocusAreas),
					IsAiGenerated = true,
					DateGenerated = DateTime.UtcNow,
					RecommendedResources = generatedPlan.RecommendedCourses.Select(c => new Domain.Entities.RecommendedResource
					{
						Title = c.CourseTitle,
						Url = c.CourseUrl,
						ResourseType = Domain.Enum.ResourseType.Course
					}).ToList()
				};

				await _unitOfWork.ImprovementPlans.AddAsync(newPlan);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
			}

			return BaseResponse<AIImprovementPlanResponseDTO>.SuccessResponse(generatedPlan, "AI Improvement Plan generated successfully.");
		}
	}
}

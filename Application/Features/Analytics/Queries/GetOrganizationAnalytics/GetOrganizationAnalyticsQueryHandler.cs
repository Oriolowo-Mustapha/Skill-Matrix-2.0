using Application.DTOs;
using Application.DTOs.Analytics;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Enum;
using MediatR;

namespace Application.Features.Analytics.Queries.GetOrganizationAnalytics
{
	public class GetOrganizationAnalyticsQueryHandler : IRequestHandler<GetOrganizationAnalyticsQuery, BaseResponse<OrganizationAnalyticsDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetOrganizationAnalyticsQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<OrganizationAnalyticsDTO>> Handle(GetOrganizationAnalyticsQuery request, CancellationToken cancellationToken)
		{
			// Verify org exists
			var org = await _unitOfWork.Organizations.GetByIdAsync(request.OrganizationId);
			if (org == null) throw new NotFoundException("Organization not found.");

			// Verify authorization (if manager, ensure they belong to this org)
			if (request.RequesterRole == Roles.Manager.ToString())
			{
				var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(request.RequesterId);
				if (manager == null || manager.OrganizationId != request.OrganizationId)
				{
					throw new UnauthorizedAccessException("You are not authorized to view analytics for this organization.");
				}
			}

			var allTeamMembers = await _unitOfWork.TeamMembers.GetAllAsync();
			var orgTeamMembers = allTeamMembers.Where(t => t.OrganizationId == request.OrganizationId).ToList();
			var teamMemberIds = orgTeamMembers.Select(t => t.Id).ToList();

			var allAssessments = await _unitOfWork.AssessmentResults.GetAllAsync();
			// Since AssessmentResult is tied to TeamMemberId, let's filter
			var orgAssessments = allAssessments.Where(a => a.TeamMemberID.HasValue && teamMemberIds.Contains(a.TeamMemberID.Value)).ToList();

			// For the 'Skill' property in AssessmentResult to be loaded, we might need to get Skills
			var allSkills = await _unitOfWork.Skills.GetAllAsync();

			var analytics = new OrganizationAnalyticsDTO
			{
				OrganizationId = request.OrganizationId,
				TotalMembers = orgTeamMembers.Count,
				TotalAssessmentsCompleted = orgAssessments.Count,
				AverageProficiencyScore = orgAssessments.Any() ? orgAssessments.Average(a => a.Score) : 0,
			};

			// Group by Skill for Distributions
			var skillGroups = orgAssessments.GroupBy(a => a.SkillId);
			foreach (var group in skillGroups)
			{
				var skill = allSkills.FirstOrDefault(s => s.Id == group.Key);
				if (skill == null) continue;

				var distribution = new SkillDistributionDTO
				{
					SkillName = skill.Name,
					NoviceCount = group.Count(g => g.ProficiencyLevel == ProficiencyLevel.Novice),
					BegineerCount = group.Count(g => g.ProficiencyLevel == ProficiencyLevel.Begineer),
					IntermediateCount = group.Count(g => g.ProficiencyLevel == ProficiencyLevel.Intermediate),
					ProficientCount = group.Count(g => g.ProficiencyLevel == ProficiencyLevel.Proficient),
					ExpertCount = group.Count(g => g.ProficiencyLevel == ProficiencyLevel.Expert)
				};
				analytics.SkillDistributions.Add(distribution);
			}

			// Top Skills (Skills with the highest average score)
			analytics.TopSkills = skillGroups
				.OrderByDescending(g => g.Average(a => a.Score))
				.Take(5)
				.Select(g => allSkills.FirstOrDefault(s => s.Id == g.Key)?.Name ?? "Unknown")
				.ToList();

			// Growth Metrics (Assessments completed per month)
			var growthMetrics = orgAssessments
				.GroupBy(a => new { a.DateCreated.Year, a.DateCreated.Month })
				.OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
				.Select(g => new TeamGrowthMetricDTO
				{
					Month = $"{g.Key.Year}-{g.Key.Month:D2}",
					AssessmentsCompleted = g.Count(),
					AverageScore = g.Average(a => a.Score)
				})
				.ToList();

			analytics.GrowthMetrics = growthMetrics;

			return BaseResponse<OrganizationAnalyticsDTO>.SuccessResponse(analytics, "Analytics retrieved successfully.");
		}
	}
}

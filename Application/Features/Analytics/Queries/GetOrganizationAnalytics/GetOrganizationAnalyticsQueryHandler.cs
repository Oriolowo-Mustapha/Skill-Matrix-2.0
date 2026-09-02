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
			var teamMemberIds = orgTeamMembers.Select(t => t.Id).ToHashSet();
			var memberNameLookup = orgTeamMembers.ToDictionary(t => t.Id, t => $"{t.FirstName} {t.LastName}");

			var allAssessments = await _unitOfWork.AssessmentResults.GetAllAsync();
			var orgAssessments = allAssessments.Where(a => a.TeamMemberID.HasValue && teamMemberIds.Contains(a.TeamMemberID.Value)).ToList();

			var allSkills = await _unitOfWork.Skills.GetAllAsync();
			var skillNameLookup = allSkills.ToDictionary(s => s.Id, s => s.Name);

			var allAssignedSkills = await _unitOfWork.AssignedSkills.GetAllAsync();
			var orgAssignedSkills = allAssignedSkills.Where(ts => ts.TeamMemberId.HasValue && teamMemberIds.Contains(ts.TeamMemberId.Value)).ToList();
			var assignedSkillNameLookup = orgAssignedSkills.ToDictionary(ts => ts.Id, ts => ts.Name.Length > 0 ? ts.Name : (ts.SkillId != Guid.Empty && skillNameLookup.ContainsKey(ts.SkillId) ? skillNameLookup[ts.SkillId] : "Skill"));

			var allPlans = await _unitOfWork.ImprovementPlans.GetAllAsync();
			var planAssessmentIds = allPlans.Where(p => p.AssessmentResultId.HasValue).Select(p => p.AssessmentResultId!.Value).ToHashSet();
			var planMemberByAssessment = orgAssessments
				.Where(a => a.TeamMemberID.HasValue && planAssessmentIds.Contains(a.Id))
				.ToDictionary(a => a.Id, a => a.TeamMemberID!.Value);

			var orgPlans = allPlans
				.Where(p =>
					(p.AssessmentResultId.HasValue && planMemberByAssessment.ContainsKey(p.AssessmentResultId!.Value)) ||
					(p.AssignedSkillId.HasValue && orgAssignedSkills.Any(ts => ts.Id == p.AssignedSkillId.Value)))
				.ToList();

			var allBadges = await _unitOfWork.Badges.GetAllAsync();
			var allAssignedBadges = await _unitOfWork.AssignedBadges.GetAllAsync();
			var orgAssignedBadges = allAssignedBadges.Where(b => b.TeamMemberId.HasValue && teamMemberIds.Contains(b.TeamMemberId.Value)).ToList();
			var badgeNameLookup = allBadges.ToDictionary(b => b.Id, b => b.Name);

			var orgSkillGaps = await _unitOfWork.SkillGaps.FindAsync(
				g => g.TeamMemberId.HasValue && teamMemberIds.Contains(g.TeamMemberId.Value));

			var analytics = new OrganizationAnalyticsDTO
			{
				OrganizationId = request.OrganizationId,
				TotalMembers = orgTeamMembers.Count,
				TotalAssessmentsCompleted = orgAssessments.Count,
				AverageProficiencyScore = orgAssessments.Any() ? orgAssessments.Average(a => a.Score) : 0,
				ActiveImprovementPlansCount = orgPlans.Count,
				BadgesAwardedCount = orgAssignedBadges.Count,
				SkillGapsCount = orgSkillGaps.Count,
				MasteredSkillsCount = orgAssignedSkills.Count(ts => ts.IsFullyMastered || ts.ProficiencyLevel == ProficiencyLevel.Expert),
			};

			// Group by Skill for Distributions (SkillId references an AssignedSkill)
			var skillGroups = orgAssessments.GroupBy(a => a.SkillId);
			foreach (var group in skillGroups)
			{
				var skillName = AssignedSkillNameOrFallback(group.Key, assignedSkillNameLookup, skillNameLookup);

				var distribution = new SkillDistributionDTO
				{
					SkillName = skillName,
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
				.Select(g => AssignedSkillNameOrFallback(g.Key, assignedSkillNameLookup, skillNameLookup))
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

			// Member summaries
			var memberSummaries = orgTeamMembers.Select(t =>
			{
				var memberAssessments = orgAssessments.Where(a => a.TeamMemberID == t.Id).ToList();
				return new MemberSummaryDTO
				{
					Id = t.Id,
					FirstName = t.FirstName,
					LastName = t.LastName,
					Email = t.Email,
					ProfilePictureUrl = t.ProfilePictureUrl ?? string.Empty,
					TotalPoints = t.TotalPoints,
					AssessmentsCompleted = memberAssessments.Count,
					AverageScore = memberAssessments.Any() ? memberAssessments.Average(a => a.Score) : 0,
					ProficiencyLevel = t.ProficiencyLevel.ToString()
				};
			}).ToList();

			analytics.TopMembers = memberSummaries
				.OrderByDescending(m => m.TotalPoints)
				.ThenByDescending(m => m.AverageScore)
				.Take(5)
				.ToList();

			analytics.WeakMembers = memberSummaries
				.Where(m => m.AssessmentsCompleted > 0)
				.OrderBy(m => m.AverageScore)
				.Take(5)
				.ToList();

			// Recent activity feed
			var activity = new List<ActivityEventDTO>();

			// Recent assessments
			foreach (var a in orgAssessments.OrderByDescending(x => x.DateCreated).Take(8))
			{
				var memberName = a.TeamMemberID.HasValue && memberNameLookup.ContainsKey(a.TeamMemberID.Value) ? memberNameLookup[a.TeamMemberID.Value] : "A member";
				activity.Add(new ActivityEventDTO
				{
					Type = "assessment",
					Action = "completed assessment",
					Description = $"scored {a.Score}% on {AssignedSkillNameOrFallback(a.SkillId, assignedSkillNameLookup, skillNameLookup)}",
					MemberName = memberName,
					MemberId = a.TeamMemberID,
					SkillOrBadgeName = AssignedSkillNameOrFallback(a.SkillId, assignedSkillNameLookup, skillNameLookup),
					Date = a.DateCreated
				});
			}

			// Recent badges
			foreach (var b in orgAssignedBadges.OrderByDescending(x => x.DateAwarded).Take(5))
			{
				var memberName = b.TeamMemberId.HasValue && memberNameLookup.ContainsKey(b.TeamMemberId.Value) ? memberNameLookup[b.TeamMemberId.Value] : "A member";
				var badgeName = badgeNameLookup.TryGetValue(b.BadgeId, out var name) ? name : "a badge";
				activity.Add(new ActivityEventDTO
				{
					Type = "badge",
					Action = "earned",
					Description = $"earned the {badgeName} badge",
					MemberName = memberName,
					MemberId = b.TeamMemberId,
					SkillOrBadgeName = badgeName,
					Date = b.DateAwarded
				});
			}

			// Recent member joins
			foreach (var m in orgTeamMembers.OrderByDescending(x => x.DateJoined).Take(5))
			{
				activity.Add(new ActivityEventDTO
				{
					Type = "member",
					Action = "joined the team",
					Description = "joined the team",
					MemberName = $"{m.FirstName} {m.LastName}",
					MemberId = m.Id,
					Date = m.DateJoined
				});
			}

			// Recent improvement plan generations
			foreach (var p in orgPlans.OrderByDescending(x => x.DateGenerated).Take(5))
			{
				Guid? memberId = null;
				if (p.AssessmentResultId.HasValue && planMemberByAssessment.ContainsKey(p.AssessmentResultId!.Value))
				{
					memberId = planMemberByAssessment[p.AssessmentResultId!.Value];
				}
				else if (p.AssignedSkillId.HasValue)
				{
					var skill = orgAssignedSkills.FirstOrDefault(ts => ts.Id == p.AssignedSkillId.Value);
					if (skill != null && skill.TeamMemberId.HasValue) memberId = skill.TeamMemberId.Value;
				}
				if (memberId == null) continue;

				var memberName = memberNameLookup.ContainsKey(memberId.Value) ? memberNameLookup[memberId.Value] : "A member";
				activity.Add(new ActivityEventDTO
				{
					Type = "plan",
					Action = "generated an improvement plan",
					Description = "generated an improvement plan",
					MemberName = memberName,
					MemberId = memberId,
					Date = p.DateGenerated
				});
			}

			analytics.RecentActivity = activity
				.OrderByDescending(x => x.Date)
				.Take(12)
				.ToList();

			return BaseResponse<OrganizationAnalyticsDTO>.SuccessResponse(analytics, "Analytics retrieved successfully.");
		}

		private static string AssignedSkillNameOrFallback(Guid skillId, IReadOnlyDictionary<Guid, string> assignedSkillNameLookup, IReadOnlyDictionary<Guid, string> skillNameLookup)
		{
			if (assignedSkillNameLookup.ContainsKey(skillId) && assignedSkillNameLookup[skillId] != "Skill")
				return assignedSkillNameLookup[skillId];
			if (skillNameLookup.ContainsKey(skillId))
				return skillNameLookup[skillId];
			return "a skill";
		}
	}
}

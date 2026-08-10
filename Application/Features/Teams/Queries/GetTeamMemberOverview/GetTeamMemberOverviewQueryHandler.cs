using Application.DTOs;
using Application.Interfaces.Repository;
using Domain.Entities;
using Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Teams.Queries.GetTeamMemberOverview
{
	public class GetTeamMemberOverviewQueryHandler : IRequestHandler<GetTeamMemberOverviewQuery, BaseResponse<TeamMemberDetailedOverviewDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetTeamMemberOverviewQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<TeamMemberDetailedOverviewDTO>> Handle(GetTeamMemberOverviewQuery request, CancellationToken cancellationToken)
		{
			var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(request.ManagerId);
			if (manager == null)
			{
				return BaseResponse<TeamMemberDetailedOverviewDTO>.FailureResponse("Manager profile not found.");
			}

			var members = await _unitOfWork.TeamMembers.FindAsync(
				m => m.Id == request.TeamMemberId && m.OrganizationId == manager.OrganizationId,
				m => m.TeamMemberSkills,
				m => m.CareerPaths,
				m => m.AssessmentResults,
				m => m.Badges
			);

			var member = members.FirstOrDefault();
			if (member == null)
			{
				return BaseResponse<TeamMemberDetailedOverviewDTO>.FailureResponse("Team member not found in your organization.");
			}

			// Skills
			var skills = member.TeamMemberSkills ?? new List<AssignedSkill>();
			var skillDtos = skills.Select(s => new TeamMemberSkillOverviewDTO
			{
				SkillId = s.SkillId,
				Name = s.Name,
				Category = s.Category,
				ProficiencyLevel = s.ProficiencyLevel.ToString(),
				IsFullyMastered = s.ProficiencyLevel == ProficiencyLevel.Expert,
				DateAssigned = s.DateAssigned
			}).ToList();

			int totalSkills = skillDtos.Count;
			int masteredSkills = skillDtos.Count(s => s.IsFullyMastered);
			int inProgressSkills = totalSkills - masteredSkills;

			// Career Paths
			var careerPaths = member.CareerPaths ?? new List<AssignedCareerPath>();
			var pathDtos = careerPaths.Select(cp => new TeamMemberCareerPathOverviewDTO
			{
				CareerPathId = cp.CareerPathId,
				Title = cp.Title,
				Description = cp.Description,
				ProgressPercentage = cp.ProgressPercentage,
				DateAssigned = cp.DateAssigned
			}).ToList();

			// Assessments
			var assessmentResults = member.AssessmentResults ?? new List<AssessmentResult>();
			int totalAssessments = assessmentResults.Count;
			double avgScore = totalAssessments > 0 ? Math.Round(assessmentResults.Average(a => a.Score), 2) : 0.0;

			var recentAssessments = assessmentResults
				.OrderByDescending(a => a.DateCreated)
				.Take(5)
				.Select(a => new TeamMemberAssessmentOverviewDTO
				{
					AssessmentResultId = a.Id,
					SkillName = a.Skill?.Name ?? "General Assessment",
					Score = a.Score,
					TotalQuestions = a.TotalQuestions,
					NoOfCorrectAnswers = a.NoOfCorrectAnswers,
					AchievedLevel = a.ProficiencyLevel.ToString(),
					DateTaken = a.DateCreated
				}).ToList();

			// Improvement Plans
			var improvementPlans = await _unitOfWork.ImprovementPlans.FindAsync(
				ip => ip.AssessmentResult != null && ip.AssessmentResult.TeamMemberID == member.Id,
				ip => ip.Tasks
			);

			var planDtos = improvementPlans.Select(ip => new TeamMemberImprovementPlanOverviewDTO
			{
				Id = ip.Id,
				FocusArea = ip.FocusArea,
				GeneratedSummary = ip.GeneratedSummary,
				DateGenerated = ip.DateGenerated,
				TotalTasks = ip.Tasks?.Count ?? 0,
				CompletedTasks = ip.Tasks?.Count(t => t.Status == "Completed" || t.CompletedAt.HasValue) ?? 0
			}).ToList();

			var overview = new TeamMemberDetailedOverviewDTO
			{
				Id = member.Id,
				FirstName = member.FirstName,
				LastName = member.LastName,
				Email = member.Email,
				Role = member.Role,
				ProfilePictureUrl = member.ProfilePictureUrl,
				DateJoined = member.DateJoined,
				TotalPoints = member.TotalPoints,
				TotalAssignedSkills = totalSkills,
				MasteredSkillsCount = masteredSkills,
				InProgressSkillsCount = inProgressSkills,
				Skills = skillDtos,
				CareerPaths = pathDtos,
				TotalAssessmentsTaken = totalAssessments,
				AverageAssessmentScore = avgScore,
				RecentAssessments = recentAssessments,
				ImprovementPlans = planDtos
			};

			return BaseResponse<TeamMemberDetailedOverviewDTO>.SuccessResponse(overview, "Team member detailed overview retrieved successfully.");
		}
	}
}

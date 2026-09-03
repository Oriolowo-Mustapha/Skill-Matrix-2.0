using Application.DTOs;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Dashboard.Queries.GetMyOverview
{
	public class GetMyOverviewQueryHandler : IRequestHandler<GetMyOverviewQuery, BaseResponse<MyOverviewDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActivityLogService _activityLogService;

		public GetMyOverviewQueryHandler(IUnitOfWork unitOfWork, IActivityLogService activityLogService)
		{
			_unitOfWork = unitOfWork;
			_activityLogService = activityLogService;
		}

		public async Task<BaseResponse<MyOverviewDTO>> Handle(GetMyOverviewQuery request, CancellationToken cancellationToken)
		{
			var isLearner = request.UserRole == Roles.Learner.ToString();

			// ---- Identity + collections (branch on role) ----
			string firstName, lastName, email, role;
			string? profilePictureUrl;
			DateTime dateJoined;
			List<AssignedSkill> skills;
			List<AssignedCareerPath> careerPaths;
			List<AssessmentResult> assessmentResults;
			List<AssignedBadge> badges;
			int totalPoints;

			if (isLearner)
			{
				var learner = await _unitOfWork.Learners.GetByIdAsync(request.UserId);
				if (learner == null)
				{
					return BaseResponse<MyOverviewDTO>.FailureResponse("Learner profile not found.");
				}

				firstName = learner.FirstName;
				lastName = learner.LastName;
				email = learner.Email;
				role = learner.Role;
				profilePictureUrl = learner.ProfilePictureUrl;
				dateJoined = learner.DateJoined;
				skills = learner.LearnerSkills ?? new List<AssignedSkill>();
				careerPaths = learner.LearnerCareerPaths ?? new List<AssignedCareerPath>();
				assessmentResults = learner.AssessmentResults ?? new List<AssessmentResult>();
				badges = learner.Badges ?? new List<AssignedBadge>();
				totalPoints = learner.TotalPoints;
			}
			else
			{
				var teamMember = await _unitOfWork.TeamMembers.GetByIdAsync(request.UserId);
				if (teamMember == null)
				{
					return BaseResponse<MyOverviewDTO>.FailureResponse("Team member profile not found.");
				}

				firstName = teamMember.FirstName;
				lastName = teamMember.LastName;
				email = teamMember.Email;
				role = teamMember.Role;
				profilePictureUrl = teamMember.ProfilePictureUrl;
				dateJoined = teamMember.DateJoined;
				skills = teamMember.TeamMemberSkills ?? new List<AssignedSkill>();
				careerPaths = teamMember.CareerPaths ?? new List<AssignedCareerPath>();
				assessmentResults = teamMember.AssessmentResults ?? new List<AssessmentResult>();
				badges = teamMember.Badges ?? new List<AssignedBadge>();
				totalPoints = teamMember.TotalPoints;
			}

			// ---- Skills summary ----
			var skillDtos = skills.Select(s => new MySkillOverviewDTO
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

			// ---- Career paths ----
			var pathDtos = careerPaths.Select(cp => new MyCareerPathOverviewDTO
			{
				CareerPathId = cp.CareerPathId,
				Title = cp.Title,
				Description = cp.Description,
				ProgressPercentage = cp.ProgressPercentage,
				DateAssigned = cp.DateAssigned
			}).ToList();

			// ---- Assessments ----
			int totalAssessments = assessmentResults.Count;
			double avgScore = totalAssessments > 0 ? Math.Round(assessmentResults.Average(a => a.Score), 2) : 0.0;

			var recentAssessments = assessmentResults
				.OrderByDescending(a => a.DateCreated)
				.Take(5)
				.Select(a => new MyAssessmentOverviewDTO
				{
					AssessmentResultId = a.Id,
					SkillName = a.Skill?.Name ?? "General Assessment",
					Score = a.Score,
					TotalQuestions = a.TotalQuestions,
					NoOfCorrectAnswers = a.NoOfCorrectAnswers,
					AchievedLevel = a.ProficiencyLevel.ToString(),
					DateTaken = a.DateCreated
				}).ToList();

			// ---- Improvement plans ----
			var improvementPlans = await _unitOfWork.ImprovementPlans.FindAsync(
				ip => ip.AssessmentResult != null && ip.AssessmentResult.LearnerID == request.UserId
					|| ip.AssessmentResult != null && ip.AssessmentResult.TeamMemberID == request.UserId,
				ip => ip.Tasks
			);

			var planDtos = improvementPlans.Select(ip => new MyImprovementPlanOverviewDTO
			{
				Id = ip.Id,
				FocusArea = ip.FocusArea,
				GeneratedSummary = ip.GeneratedSummary,
				DateGenerated = ip.DateGenerated,
				TotalTasks = ip.Tasks?.Count ?? 0,
				CompletedTasks = ip.Tasks?.Count(t => t.Status == "Completed" || t.CompletedAt.HasValue) ?? 0
			}).ToList();

			// ---- Gamification (streak from entity, XP from level table) ----
			var now = DateTime.UtcNow;
			var activityLogs = await _unitOfWork.UserActivityLogs.FindAsync(
				l => l.UserId == request.UserId && l.UserRole == request.UserRole
			);

			var streakEntity = await _activityLogService.GetStreakAsync(request.UserId, request.UserRole, cancellationToken);
			var xpLevel = await _activityLogService.GetXpLevelForPointsAsync(totalPoints, cancellationToken);
			DateTime lastActivity = activityLogs.Count == 0 ? DateTime.MinValue : activityLogs.Max(l => l.CreatedAt);

			var recentActivity = activityLogs
				.OrderByDescending(l => l.CreatedAt)
				.Take(10)
				.Select(l => new RecentActivityDTO
				{
					Id = l.Id,
					ActivityType = l.ActivityType.ToString(),
					Description = l.Description,
					PointsEarned = l.PointsEarned,
					CreatedAt = l.CreatedAt
				}).ToList();

			var todayLogs = activityLogs.Where(l => l.CreatedAt.Date == now.Date).ToList();
			var dailyQuests = BuildDailyQuests(todayLogs, now);

			var streakDto = streakEntity != null ? new StreakDTO
			{
				CurrentStreak = streakEntity.CurrentStreak,
				LongestStreak = streakEntity.LongestStreak,
				LastActivityDate = streakEntity.LastActivityDate,
				FreezeTokens = streakEntity.FreezeTokens,
				StreakStartDate = streakEntity.StreakStartDate,
				IsBroken = streakEntity.BrokenDate.HasValue,
				BrokenDate = streakEntity.BrokenDate
			} : new StreakDTO { CurrentStreak = 0, LongestStreak = 0, FreezeTokens = 0, IsBroken = false };

			var overview = new MyOverviewDTO
			{
				Id = request.UserId,
				FirstName = firstName,
				LastName = lastName,
				Email = email,
				Role = role,
				ProfilePictureUrl = profilePictureUrl,
				DateJoined = dateJoined,
				TotalPoints = totalPoints,
				CurrentStreak = streakDto.CurrentStreak,
				Streak = streakDto,
				Level = xpLevel?.Level,
				LevelTitle = xpLevel?.Title,
				BadgeCount = badges.Count,
				LastActivityDate = lastActivity,
				DailyQuests = dailyQuests,
				RecentActivity = recentActivity,
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

			return BaseResponse<MyOverviewDTO>.SuccessResponse(overview, "Overview retrieved successfully.");
		}

		private static List<DailyQuestDTO> BuildDailyQuests(List<UserActivityLog> todayLogs, DateTime now)
		{
			int todayCompletions = todayLogs.Count(l => l.ActivityType == UserActivityType.AssessmentCompleted
				|| l.ActivityType == UserActivityType.ImprovementTaskCompleted);
			int todayEndorsements = todayLogs.Count(l => l.ActivityType == UserActivityType.PeerEndorsed);
			int todaySkillMastered = todayLogs.Count(l => l.ActivityType == UserActivityType.SkillMastered);
			int todayPoints = todayLogs.Sum(l => l.PointsEarned);

			return new List<DailyQuestDTO>
			{
				new DailyQuestDTO
				{
					Title = "Complete an assessment",
					Description = "Complete any assessment or improvement check to earn XP.",
					Target = 1,
					Progress = Math.Min(todayCompletions, 1),
					XpReward = 40,
					Completed = todayCompletions >= 1
				},
				new DailyQuestDTO
				{
					Title = "Master a skill",
					Description = "Reach Expert level on an assigned skill.",
					Target = 1,
					Progress = Math.Min(todaySkillMastered, 1),
					XpReward = 30,
					Completed = todaySkillMastered >= 1
				},
				new DailyQuestDTO
				{
					Title = "Endorse a peer",
					Description = "Recognize a teammate's skill.",
					Target = 1,
					Progress = Math.Min(todayEndorsements, 1),
					XpReward = 10,
					Completed = todayEndorsements >= 1
				},
				new DailyQuestDTO
				{
					Title = "Earn XP today",
					Description = "Earn at least 50 XP through any activity.",
					Target = 50,
					Progress = Math.Min(todayPoints, 50),
					XpReward = 20,
					Completed = todayPoints >= 50
				}
			};
		}
	}
}
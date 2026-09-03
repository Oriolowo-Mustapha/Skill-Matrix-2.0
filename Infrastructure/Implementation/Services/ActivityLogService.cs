using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;

namespace Infrastructure.Implementation.Services
{
	public class ActivityLogService : IActivityLogService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ActivityLogService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<int> AwardPointsAsync(Guid userId, string userRole, UserActivityType activityType, string description, int points, string? entityType = null, Guid? entityId = null, CancellationToken cancellationToken = default)
		{
			if (points > 0)
			{
				await CreditPointsToUserAsync(userId, userRole, points);
			}

			await LogAsync(userId, userRole, activityType, description, entityType, entityId, points, cancellationToken);

			// Update streak after every activity
			await UpdateStreakOnActivityAsync(userId, userRole, DateTime.UtcNow, cancellationToken);

			return points;
		}

		public async Task LogAsync(Guid userId, string userRole, UserActivityType activityType, string description, string? entityType = null, Guid? entityId = null, CancellationToken cancellationToken = default)
		{
			await LogAsync(userId, userRole, activityType, description, entityType, entityId, 0, cancellationToken);
		}

		private async Task LogAsync(Guid userId, string userRole, UserActivityType activityType, string description, string? entityType, Guid? entityId, int points, CancellationToken cancellationToken)
		{
			var entry = new UserActivityLog
			{
				UserId = userId,
				UserRole = userRole,
				ActivityType = activityType,
				Description = description,
				EntityType = entityType,
				EntityId = entityId,
				PointsEarned = points,
				CreatedAt = DateTime.UtcNow
			};

			await _unitOfWork.UserActivityLogs.AddAsync(entry);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
		}

		private async Task CreditPointsToUserAsync(Guid userId, string userRole, int points)
		{
			if (userRole == Roles.Learner.ToString())
			{
				var learner = await _unitOfWork.Learners.GetByIdAsync(userId);
				if (learner != null)
				{
					learner.TotalPoints += points;
					await _unitOfWork.Learners.UpdateAsync(learner);
				}
			}
			else
			{
				var teamMember = await _unitOfWork.TeamMembers.GetByIdAsync(userId);
				if (teamMember != null)
				{
					teamMember.TotalPoints += points;
					await _unitOfWork.TeamMembers.UpdateAsync(teamMember);
				}
			}
		}

		public async Task UpdateStreakOnActivityAsync(Guid userId, string userRole, DateTime utcNow, CancellationToken cancellationToken = default)
		{
			var streak = await GetOrCreateStreakAsync(userId, userRole, cancellationToken);
			var today = utcNow.Date;

			// No previous activity — first ever
			if (streak.LastActivityDate == null)
			{
				streak.CurrentStreak = 1;
				streak.LongestStreak = 1;
				streak.StreakStartDate = utcNow;
				streak.LastActivityDate = utcNow;
				streak.UpdatedAt = utcNow;
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				return;
			}

			var lastDate = streak.LastActivityDate.Value.Date;

			// Same day — no change
			if (today == lastDate)
			{
				return;
			}

			var daysDiff = (today - lastDate).Days;

			// Next day — increment streak
			if (daysDiff == 1)
			{
				streak.CurrentStreak += 1;
				streak.LastActivityDate = utcNow;
				streak.UpdatedAt = utcNow;

				if (streak.CurrentStreak > streak.LongestStreak)
				{
					streak.LongestStreak = streak.CurrentStreak;
				}

				await _unitOfWork.SaveChangesAsync(cancellationToken);
				return;
			}

			// Gap > 1 day — streak breaks unless freeze token available
			if (streak.FreezeTokens > 0)
			{
				// Auto-consume freeze token
				streak.FreezeTokens -= 1;
				streak.LastFreezeUsedDate = utcNow;
				streak.LastActivityDate = utcNow;
				streak.UpdatedAt = utcNow;
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				return;
			}

			// Streak breaks
			streak.PreviousStreakCount = streak.CurrentStreak;
			streak.CurrentStreak = 1;
			streak.BrokenDate = utcNow;
			streak.LastActivityDate = utcNow;
			streak.StreakStartDate = utcNow;
			streak.UpdatedAt = utcNow;
			await _unitOfWork.SaveChangesAsync(cancellationToken);
		}

		public async Task<bool> FreezeStreakAsync(Guid userId, string userRole, CancellationToken cancellationToken = default)
		{
			var streak = await GetOrCreateStreakAsync(userId, userRole, cancellationToken);

			if (streak.FreezeTokens <= 0)
			{
				return false;
			}

			streak.FreezeTokens -= 1;
			streak.LastFreezeUsedDate = DateTime.UtcNow;
			streak.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<(bool Success, string Message)> RepairStreakAsync(Guid userId, string userRole, CancellationToken cancellationToken = default)
		{
			var streak = await GetOrCreateStreakAsync(userId, userRole, cancellationToken);

			if (!streak.BrokenDate.HasValue)
			{
				return (false, "Streak is not broken. Nothing to repair.");
			}

			const int repairCost = 500;

			// Check if user has enough XP
			int currentPoints = 0;
			if (userRole == Roles.Learner.ToString())
			{
				var learner = await _unitOfWork.Learners.GetByIdAsync(userId);
				if (learner == null) return (false, "User not found.");
				currentPoints = learner.TotalPoints;
			}
			else
			{
				var teamMember = await _unitOfWork.TeamMembers.GetByIdAsync(userId);
				if (teamMember == null) return (false, "User not found.");
				currentPoints = teamMember.TotalPoints;
			}

			if (currentPoints < repairCost)
			{
				return (false, $"Insufficient XP. Streak repair costs {repairCost} XP. You have {currentPoints} XP.");
			}

			// Deduct XP
			if (userRole == Roles.Learner.ToString())
			{
				var learner = await _unitOfWork.Learners.GetByIdAsync(userId);
				if (learner != null)
				{
					learner.TotalPoints -= repairCost;
					await _unitOfWork.Learners.UpdateAsync(learner);
				}
			}
			else
			{
				var teamMember = await _unitOfWork.TeamMembers.GetByIdAsync(userId);
				if (teamMember != null)
				{
					teamMember.TotalPoints -= repairCost;
					await _unitOfWork.TeamMembers.UpdateAsync(teamMember);
				}
			}

			// Restore streak
			streak.CurrentStreak = streak.PreviousStreakCount > 0 ? streak.PreviousStreakCount : 1;
			streak.BrokenDate = null;
			streak.PreviousStreakCount = 0;
			streak.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return (true, $"Streak repaired! Restored to {streak.CurrentStreak} days. ({repairCost} XP deducted)");
		}

		public async Task<UserStreak?> GetStreakAsync(Guid userId, string userRole, CancellationToken cancellationToken = default)
		{
			var streaks = await _unitOfWork.UserStreaks.FindAsync(
				s => s.UserId == userId && s.UserRole == userRole
			);
			return streaks.FirstOrDefault();
		}

		private async Task<UserStreak> GetOrCreateStreakAsync(Guid userId, string userRole, CancellationToken cancellationToken)
		{
			var existing = await GetStreakAsync(userId, userRole, cancellationToken);
			if (existing != null) return existing;

			var newStreak = new UserStreak
			{
				UserId = userId,
				UserRole = userRole,
				CurrentStreak = 0,
				LongestStreak = 0,
				FreezeTokens = 0,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _unitOfWork.UserStreaks.AddAsync(newStreak);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return newStreak;
		}

		public async Task<XpLevel?> GetXpLevelForPointsAsync(int totalPoints, CancellationToken cancellationToken = default)
		{
			var levels = await _unitOfWork.XpLevels.FindAsync(l => true);
			return levels
				.OrderByDescending(l => l.Level)
				.FirstOrDefault(l => totalPoints >= l.MinXp);
		}

		public async Task<List<XpAction>> GetXpActionsAsync(CancellationToken cancellationToken = default)
		{
			return (await _unitOfWork.XpActions.FindAsync(a => true)).ToList();
		}

		public async Task<List<XpLevel>> GetXpLevelsAsync(CancellationToken cancellationToken = default)
		{
			return (await _unitOfWork.XpLevels.FindAsync(l => true))
				.OrderBy(l => l.Level)
				.ToList();
		}

		// Keep legacy method for backward compatibility (deprecated)
		public async Task<int> GetCurrentStreakAsync(Guid userId, string userRole, DateTime utcNow)
		{
			var streak = await GetStreakAsync(userId, userRole);
			return streak?.CurrentStreak ?? 0;
		}

		public async Task<DateTime> GetLastActivityDateAsync(Guid userId, string userRole)
		{
			var logs = await _unitOfWork.UserActivityLogs.FindAsync(
				l => l.UserId == userId && l.UserRole == userRole
			);

			return logs.Count == 0 ? DateTime.MinValue : logs.Max(l => l.CreatedAt);
		}
	}
}

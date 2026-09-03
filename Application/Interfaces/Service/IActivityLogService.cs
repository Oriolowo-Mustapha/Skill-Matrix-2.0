using Domain.Entities;
using Domain.Enum;

namespace Application.Interfaces.Service
{
	public interface IActivityLogService
	{
		Task<int> AwardPointsAsync(Guid userId, string userRole, UserActivityType activityType, string description, int points, string? entityType = null, Guid? entityId = null, CancellationToken cancellationToken = default);

		Task LogAsync(Guid userId, string userRole, UserActivityType activityType, string description, string? entityType = null, Guid? entityId = null, CancellationToken cancellationToken = default);

		Task<int> GetCurrentStreakAsync(Guid userId, string userRole, DateTime utcNow);

		Task<DateTime> GetLastActivityDateAsync(Guid userId, string userRole);

		Task UpdateStreakOnActivityAsync(Guid userId, string userRole, DateTime utcNow, CancellationToken cancellationToken = default);

		Task<bool> FreezeStreakAsync(Guid userId, string userRole, CancellationToken cancellationToken = default);

		Task<(bool Success, string Message)> RepairStreakAsync(Guid userId, string userRole, CancellationToken cancellationToken = default);

		Task<UserStreak?> GetStreakAsync(Guid userId, string userRole, CancellationToken cancellationToken = default);

		Task<XpLevel?> GetXpLevelForPointsAsync(int totalPoints, CancellationToken cancellationToken = default);

		Task<List<XpAction>> GetXpActionsAsync(CancellationToken cancellationToken = default);

		Task<List<XpLevel>> GetXpLevelsAsync(CancellationToken cancellationToken = default);
	}
}
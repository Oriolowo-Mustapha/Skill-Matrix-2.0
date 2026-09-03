using Application.DTOs;
using Application.Interfaces.Service;
using MediatR;

namespace Application.Features.Gamification.Queries.GetStreak
{
	public class GetStreakQueryHandler : IRequestHandler<GetStreakQuery, BaseResponse<StreakDTO>>
	{
		private readonly IActivityLogService _activityLogService;

		public GetStreakQueryHandler(IActivityLogService activityLogService)
		{
			_activityLogService = activityLogService;
		}

		public async Task<BaseResponse<StreakDTO>> Handle(GetStreakQuery request, CancellationToken cancellationToken)
		{
			var streak = await _activityLogService.GetStreakAsync(request.UserId, request.UserRole, cancellationToken);

			if (streak == null)
			{
				var emptyStreak = new StreakDTO
				{
					CurrentStreak = 0,
					LongestStreak = 0,
					FreezeTokens = 0,
					IsBroken = false
				};
				return BaseResponse<StreakDTO>.SuccessResponse(emptyStreak, "No streak data found.");
			}

			var dto = new StreakDTO
			{
				CurrentStreak = streak.CurrentStreak,
				LongestStreak = streak.LongestStreak,
				LastActivityDate = streak.LastActivityDate,
				FreezeTokens = streak.FreezeTokens,
				StreakStartDate = streak.StreakStartDate,
				IsBroken = streak.BrokenDate.HasValue,
				BrokenDate = streak.BrokenDate
			};

			return BaseResponse<StreakDTO>.SuccessResponse(dto, "Streak retrieved successfully.");
		}
	}
}

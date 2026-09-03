using Application.DTOs;
using Application.Interfaces.Service;
using MediatR;

namespace Application.Features.Gamification.Commands.FreezeStreak
{
	public class FreezeStreakCommandHandler : IRequestHandler<FreezeStreakCommand, BaseResponse<bool>>
	{
		private readonly IActivityLogService _activityLogService;

		public FreezeStreakCommandHandler(IActivityLogService activityLogService)
		{
			_activityLogService = activityLogService;
		}

		public async Task<BaseResponse<bool>> Handle(FreezeStreakCommand request, CancellationToken cancellationToken)
		{
			var success = await _activityLogService.FreezeStreakAsync(request.UserId, request.UserRole, cancellationToken);

			if (success)
			{
				return BaseResponse<bool>.SuccessResponse(true, "Streak freeze applied successfully.");
			}

			return BaseResponse<bool>.FailureResponse("Unable to apply streak freeze. Check if you have freeze tokens available.");
		}
	}
}

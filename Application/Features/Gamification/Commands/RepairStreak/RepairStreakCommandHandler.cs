using Application.DTOs;
using Application.Interfaces.Service;
using MediatR;

namespace Application.Features.Gamification.Commands.RepairStreak
{
	public class RepairStreakCommandHandler : IRequestHandler<RepairStreakCommand, BaseResponse<RepairStreakResponseDTO>>
	{
		private readonly IActivityLogService _activityLogService;

		public RepairStreakCommandHandler(IActivityLogService activityLogService)
		{
			_activityLogService = activityLogService;
		}

		public async Task<BaseResponse<RepairStreakResponseDTO>> Handle(RepairStreakCommand request, CancellationToken cancellationToken)
		{
			var (success, message) = await _activityLogService.RepairStreakAsync(request.UserId, request.UserRole, cancellationToken);

			var response = new RepairStreakResponseDTO
			{
				Success = success,
				Message = message,
				XpCost = 500
			};

			if (success)
			{
				var streak = await _activityLogService.GetStreakAsync(request.UserId, request.UserRole, cancellationToken);
				response = response with { NewStreak = streak?.CurrentStreak ?? 0 };
				return BaseResponse<RepairStreakResponseDTO>.SuccessResponse(response, message);
			}

			return BaseResponse<RepairStreakResponseDTO>.FailureResponse(message);
		}
	}
}

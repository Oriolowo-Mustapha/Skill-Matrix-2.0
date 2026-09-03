using Application.DTOs;
using Application.Interfaces.Service;
using MediatR;

namespace Application.Features.Gamification.Queries.GetXpConfig
{
	public class GetXpConfigQueryHandler : IRequestHandler<GetXpConfigQuery, BaseResponse<XpConfigDTO>>
	{
		private readonly IActivityLogService _activityLogService;

		public GetXpConfigQueryHandler(IActivityLogService activityLogService)
		{
			_activityLogService = activityLogService;
		}

		public async Task<BaseResponse<XpConfigDTO>> Handle(GetXpConfigQuery request, CancellationToken cancellationToken)
		{
			var actions = await _activityLogService.GetXpActionsAsync(cancellationToken);
			var levels = await _activityLogService.GetXpLevelsAsync(cancellationToken);

			var config = new XpConfigDTO
			{
				Actions = actions.Select(a => new XpActionDTO
				{
					ActionType = a.ActionType,
					BaseXp = a.BaseXp,
					Description = a.Description
				}).ToList(),
				Levels = levels.Select(l => new XpLevelDTO
				{
					Level = l.Level,
					MinXp = l.MinXp,
					Title = l.Title
				}).ToList()
			};

			return BaseResponse<XpConfigDTO>.SuccessResponse(config, "XP configuration retrieved successfully.");
		}
	}
}

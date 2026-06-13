using Application.DTOs;
using MediatR;

namespace Application.Features.Gamification.Queries.GetLeaderboard
{
	public class GetLeaderboardQuery : IRequest<BaseResponse<List<LeaderboardEntryDTO>>>
	{
		public Guid OrganizationId { get; set; }
	}
}

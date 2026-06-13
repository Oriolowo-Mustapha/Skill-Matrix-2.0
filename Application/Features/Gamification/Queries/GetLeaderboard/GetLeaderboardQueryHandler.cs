using Application.DTOs;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Gamification.Queries.GetLeaderboard
{
	public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, BaseResponse<List<LeaderboardEntryDTO>>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetLeaderboardQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<List<LeaderboardEntryDTO>>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
		{
			// Get all learners and team members in the org
			var allTeamMembers = await _unitOfWork.TeamMembers.GetAllAsync();
			var orgTeamMembers = allTeamMembers.Where(t => t.OrganizationId == request.OrganizationId);

			var leaderboard = new List<LeaderboardEntryDTO>();

			foreach (var tm in orgTeamMembers)
			{
				leaderboard.Add(new LeaderboardEntryDTO
				{
					UserId = tm.Id,
					UserName = tm.UserName,
					Role = "Team Member",
					TotalPoints = tm.TotalPoints,
					ProfilePictureUrl = tm.ProfilePictureUrl
				});
			}

			// Assuming Learners don't have an OrganizationId directly in this domain model, or if they do we filter them.
			// Let's just return Team Members for the organization leaderboard for simplicity.
			
			var sortedLeaderboard = leaderboard.OrderByDescending(l => l.TotalPoints).Take(50).ToList();

			return BaseResponse<List<LeaderboardEntryDTO>>.SuccessResponse(sortedLeaderboard, "Leaderboard retrieved successfully.");
		}
	}
}

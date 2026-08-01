using Application.DTOs;
using Application.Interfaces.Repository;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Teams.Queries.GetTeamMembers
{
	public class GetTeamMembersQueryHandler : IRequestHandler<GetTeamMembersQuery, List<TeamMemberDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetTeamMembersQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<List<TeamMemberDTO>> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
		{
			var members = await _unitOfWork.TeamMembers.FindAsync(
				m => m.ManagerId == request.ManagerId,
				m => m.CareerPaths
			);

			return members.Select(m => new TeamMemberDTO
			{
				Id = m.Id,
				FirstName = m.FirstName,
				LastName = m.LastName,
				Email = m.Email,
				UserName = m.UserName,
				ProfilePicUrl = m.ProfilePictureUrl,
				OrganizationId = m.OrganizationId,
				ManagerId = m.ManagerId,
				AssignedCareerPathIds = m.CareerPaths?.Select(cp => cp.CareerPathId).ToList() ?? new List<Guid>()
			}).ToList();
		}
	}
}

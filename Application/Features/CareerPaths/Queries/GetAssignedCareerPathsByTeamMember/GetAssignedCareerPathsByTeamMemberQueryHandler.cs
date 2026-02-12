using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.CareerPaths.Queries.GetAssignedCareerPathsByTeamMember
{
	public class GetAssignedCareerPathsByTeamMemberQueryHandler : IRequestHandler<GetAssignedCareerPathsByTeamMemberQuery, List<AssignedCareerPathDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetAssignedCareerPathsByTeamMemberQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<List<AssignedCareerPathDTO>> Handle(GetAssignedCareerPathsByTeamMemberQuery request, CancellationToken cancellationToken)
		{
			var teamMember = await _unitOfWork.TeamMembers.GetByIdAsync(request.TeamMemberId);
			if (teamMember == null)
			{
				throw new NotFoundException($"TeamMember with ID {request.TeamMemberId} not found.");
			}

			var assignedCareerPaths = await _unitOfWork.AssignedCareerPaths.FindAsync(
				acp => acp.TeamMemberId == request.TeamMemberId);

			return assignedCareerPaths.Select(acp => new AssignedCareerPathDTO
			{
				Id = acp.Id,
				Title = acp.Title,
				Description = acp.Description,
				ImageUrl = acp.ImageUrl,
				CareerPathId = acp.CareerPathId,
				DateAssigned = acp.DateAssigned
			}).ToList();
		}
	}
}
using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Badges.Commands.UnassignBadgeFromTeamMember
{
	public class UnassignBadgeFromTeamMemberCommandHandler : IRequestHandler<UnassignBadgeFromTeamMemberCommand, BaseResponse<string>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public UnassignBadgeFromTeamMemberCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<string>> Handle(UnassignBadgeFromTeamMemberCommand request, CancellationToken cancellationToken)
		{
			var assignedBadge = (await _unitOfWork.AssignedBadges.GetAllAsync())
								.FirstOrDefault(ab => ab.BadgeId == request.BadgeId && ab.TeamMemberId == request.TeamMemberId);

			if (assignedBadge == null)
			{
				throw new NotFoundException($"Badge with ID '{request.BadgeId}' is not assigned to Team Member with ID '{request.TeamMemberId}'.");
			}

			await _unitOfWork.AssignedBadges.DeleteAsync(assignedBadge);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return BaseResponse<string>.SuccessResponse(" ", "Badge successfully unassigned from team member.");
		}
	}
}

using Application.Features.Badges.Commands.UnassignBadgeFromTeamMember;
using Application.Interfaces.Repository;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities; // Needed for Badge and TeamMember entities

namespace Application.Features.Badges.Commands.UnassignBadgeFromTeamMember
{
	public class UnassignBadgeFromTeamMemberCommandValidator : AbstractValidator<UnassignBadgeFromTeamMemberCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		public UnassignBadgeFromTeamMemberCommandValidator(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;

			RuleFor(p => p.BadgeId)
				.NotEmpty().WithMessage("{PropertyName} is required.")
				.NotNull()
				.MustAsync(BadgeExists).WithMessage("Badge with this ID does not exist.");

			RuleFor(p => p.TeamMemberId)
				.NotEmpty().WithMessage("{PropertyName} is required.")
				.NotNull()
				.MustAsync(TeamMemberExists).WithMessage("Team Member with this ID does not exist.");

            RuleFor(p => p)
                .MustAsync(AssignmentExists).WithMessage("This badge is not currently assigned to this team member.");
		}

		private async Task<bool> BadgeExists(Guid id, CancellationToken cancellationToken)
		{
			return await _unitOfWork.Badges.ExistsAsync(b => b.Id == id);
		}

		private async Task<bool> TeamMemberExists(Guid id, CancellationToken cancellationToken)
		{
			return await _unitOfWork.TeamMembers.ExistsAsync(tm => tm.Id == id);
		}

        private async Task<bool> AssignmentExists(UnassignBadgeFromTeamMemberCommand command, CancellationToken cancellationToken)
        {
            return await _unitOfWork.AssignedBadges.ExistsAsync(
                ab => ab.BadgeId == command.BadgeId && ab.TeamMemberId == command.TeamMemberId);
        }
	}
}

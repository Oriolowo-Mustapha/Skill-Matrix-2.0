using Application.Interfaces.Repository;
using FluentValidation;

namespace Application.Features.Badges.Commands.AssignBadgeToTeamMember
{
	public class AssignBadgeToTeamMemberCommandValidator : AbstractValidator<AssignBadgeToTeamMemberCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		public AssignBadgeToTeamMemberCommandValidator(IUnitOfWork unitOfWork)
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
				.MustAsync(BeUniqueAssignment).WithMessage("This badge is already assigned to this team member.");
		}

		private async Task<bool> BadgeExists(Guid id, CancellationToken cancellationToken)
		{
			return await _unitOfWork.Badges.ExistsAsync(b => b.Id == id);
		}

		private async Task<bool> TeamMemberExists(Guid id, CancellationToken cancellationToken)
		{
			return await _unitOfWork.TeamMembers.ExistsAsync(tm => tm.Id == id);
		}

		private async Task<bool> BeUniqueAssignment(AssignBadgeToTeamMemberCommand command, CancellationToken cancellationToken)
		{
			return !await _unitOfWork.AssignedBadges.ExistsAsync(
				ab => ab.BadgeId == command.BadgeId && ab.TeamMemberId == command.TeamMemberId);
		}
	}
}

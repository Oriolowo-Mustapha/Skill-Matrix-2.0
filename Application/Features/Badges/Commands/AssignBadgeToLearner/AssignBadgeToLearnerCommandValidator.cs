using Application.Features.Badges.Commands.AssignBadgeToLearner;
using Application.Interfaces.Repository;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities; // Needed for Badge and Learner entities

namespace Application.Features.Badges.Commands.AssignBadgeToLearner
{
	public class AssignBadgeToLearnerCommandValidator : AbstractValidator<AssignBadgeToLearnerCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		public AssignBadgeToLearnerCommandValidator(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;

			RuleFor(p => p.BadgeId)
				.NotEmpty().WithMessage("{PropertyName} is required.")
				.NotNull()
				.MustAsync(BadgeExists).WithMessage("Badge with this ID does not exist.");

			RuleFor(p => p.LearnerId)
				.NotEmpty().WithMessage("{PropertyName} is required.")
				.NotNull()
				.MustAsync(LearnerExists).WithMessage("Learner with this ID does not exist.");

            RuleFor(p => p)
                .MustAsync(BeUniqueAssignment).WithMessage("This badge is already assigned to this learner.");
		}

		private async Task<bool> BadgeExists(Guid id, CancellationToken cancellationToken)
		{
			return await _unitOfWork.Badges.ExistsAsync(b => b.Id == id);
		}

		private async Task<bool> LearnerExists(Guid id, CancellationToken cancellationToken)
		{
			return await _unitOfWork.Learners.ExistsAsync(l => l.Id == id);
		}

        private async Task<bool> BeUniqueAssignment(AssignBadgeToLearnerCommand command, CancellationToken cancellationToken)
        {
            return !await _unitOfWork.AssignedBadges.ExistsAsync(
                ab => ab.BadgeId == command.BadgeId && ab.LearnerID == command.LearnerId);
        }
	}
}

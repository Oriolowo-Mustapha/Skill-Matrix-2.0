using Application.Features.Badges.Commands.UpdateBadge;
using Application.Interfaces.Repository;
using Application.Extensions;
using Domain.Enum;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities; // Needed for the Badge entity in ExistsAsync

namespace Application.Features.Badges.Commands.UpdateBadge
{
	public class UpdateBadgeCommandValidator : AbstractValidator<UpdateBadgeCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		public UpdateBadgeCommandValidator(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;

			RuleFor(p => p.Id)
				.NotEmpty().WithMessage("{PropertyName} is required.")
				.NotNull()
				.MustAsync(BadgeExists).WithMessage("Badge with this ID does not exist.");

			RuleFor(p => p.Name)
				.NotEmpty().WithMessage("{PropertyName} is required.")
				.NotNull()
				.MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.")
				.MustAsync(BeUniqueBadgeNameExcludingCurrent).WithMessage("A badge with this name already exists.");

			RuleFor(p => p.Description)
				.NotEmpty().WithMessage("{PropertyName} is required.")
				.NotNull()
				.MaximumLength(250).WithMessage("{PropertyName} must not exceed 250 characters.");

			RuleFor(p => p.Icon)
				.IsValidImage();

			RuleFor(p => p.Criteria)
				.NotEmpty().WithMessage("{PropertyName} is required.")
				.NotNull()
				.MaximumLength(500).WithMessage("{PropertyName} must not exceed 500 characters.");

			RuleFor(p => p.ProficiencyLevel)
				.NotEmpty().WithMessage("{PropertyName} is required.")
				.NotNull()
				.Must(BeValidProficiencyLevel).WithMessage("{PropertyName} must be a valid proficiency level.");
		}

		private async Task<bool> BadgeExists(Guid id, CancellationToken cancellationToken)
		{
			return await _unitOfWork.Badges.ExistsAsync(b => b.Id == id);
		}

		private async Task<bool> BeUniqueBadgeNameExcludingCurrent(UpdateBadgeCommand command, string name, CancellationToken cancellationToken)
		{
            // Check if any other badge (excluding the current one being updated) has the same name
            return !await _unitOfWork.Badges.ExistsAsync(b => b.Name == name && b.Id != command.Id);
		}

		private bool BeValidProficiencyLevel(string proficiencyLevel)
		{
			return Enum.TryParse(typeof(ProficiencyLevel), proficiencyLevel, true, out _);
		}
	}
}

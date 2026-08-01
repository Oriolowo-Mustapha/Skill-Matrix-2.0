using Application.Interfaces.Repository;
using Application.Extensions;
using Domain.Enum;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Badges.Commands.CreateBadge
{
	public class CreateBadgeCommandValidator : AbstractValidator<CreateBadgeCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		public CreateBadgeCommandValidator(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;

			RuleFor(p => p.Name)
				.NotEmpty().WithMessage("{PropertyName} is required.")
				.NotNull()
				.MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.")
				.MustAsync(BeUniqueBadgeName).WithMessage("A badge with this name already exists.");

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

		private async Task<bool> BeUniqueBadgeName(string name, CancellationToken cancellationToken)
		{
			return !await _unitOfWork.Badges.ExistsAsync(b => b.Name == name);
		}

		private bool BeValidProficiencyLevel(string proficiencyLevel)
		{
			return Enum.TryParse(typeof(ProficiencyLevel), proficiencyLevel, true, out _);
		}
	}
}

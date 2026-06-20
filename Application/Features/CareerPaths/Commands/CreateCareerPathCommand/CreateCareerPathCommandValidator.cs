using Application.Interfaces.Repository;
using Application.Extensions;
using FluentValidation;

namespace Application.Features.CareerPaths.Commands.CreateCareerPathCommand

{
	public class CreateCareerPathCommandValidator : AbstractValidator<CreateCareerPathCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		public CreateCareerPathCommandValidator(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;

			RuleFor(c => c.Title)
				.NotEmpty().WithMessage("{PropertyName} is Required")
				.NotNull()
				.MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.")
				.MustAsync(BeUniqueCareerPathTitle).WithMessage("A career path with this title already exists");

			RuleFor(c => c.Description)
				.NotEmpty().WithMessage("{PropertyName} is Required.")
				.NotNull()
				.MaximumLength(500).WithMessage("{PropertyName} must not exceeds 500 characters.");
			RuleFor(c => c.Icon)
				.IsValidImage();
		}

		private async Task<bool> BeUniqueCareerPathTitle(string title, CancellationToken cancellationToken)
		{
			return !await _unitOfWork.CareerPaths.ExistsAsync(c => c.Title == title);
		}
	}
}

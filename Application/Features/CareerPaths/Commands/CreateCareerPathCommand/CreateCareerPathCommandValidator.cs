using Application.Interfaces.Repository;
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
			RuleFor(c => c.IconURL)
				.MaximumLength(250).WithMessage("{PropertyName} must not exceeds 250 characters.")
				.Must(BeAValidUrlOrEmpty);
		}

		private async Task<bool> BeUniqueCareerPathTitle(string title, CancellationToken cancellationToken)
		{
			return await _unitOfWork.CareerPaths.ExistsAsync(c => c.Title == title);
		}

		private bool BeAValidUrlOrEmpty(string? url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return true;
			}
			return Uri.TryCreate(url, UriKind.Absolute, out _);
		}
	}
}

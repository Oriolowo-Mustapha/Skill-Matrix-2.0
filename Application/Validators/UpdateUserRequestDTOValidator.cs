using FluentValidation;

namespace Application.DTOs.Validators
{
	public class UpdateUserRequestDTOValidator : AbstractValidator<UpdateUserRequestDTO>
	{
		public UpdateUserRequestDTOValidator()
		{
			RuleFor(x => x.FirstName)
				.NotEmpty().WithMessage("First Name is required.")
				.MaximumLength(50).WithMessage("First Name cannot exceed 50 characters.");

			RuleFor(x => x.LastName)
				.NotEmpty().WithMessage("Last Name is required.")
				.MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters.");

			RuleFor(x => x.ProfilePictureUrl)
				.Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
				.When(x => !string.IsNullOrEmpty(x.ProfilePictureUrl))
				.WithMessage("Profile Picture must be a valid URL.");
		}
	}
}
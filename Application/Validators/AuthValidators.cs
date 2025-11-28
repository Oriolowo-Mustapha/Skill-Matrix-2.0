using FluentValidation;

namespace Application.DTOs.Validators
{
	public class LoginRequestDTOValidator : AbstractValidator<LoginRequestDTO>
	{
		public LoginRequestDTOValidator()
		{
			RuleFor(x => x)
				.Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.UserName))
				.WithMessage("You must provide either an Email or a Username.");

			RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Password is required.");
		}
	}

	public class RegisterLearnerRequestDTOValidator : AbstractValidator<RegisterLearnerRequestDTO>
	{
		// private readonly ILearnerRepository _learnerRepository; // Inject your repo

		public RegisterLearnerRequestDTOValidator(/* ILearnerRepository learnerRepository */)
		{
			// _learnerRepository = learnerRepository;

			RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
			RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
			RuleFor(x => x.UserName).NotEmpty().MinimumLength(3);

			RuleFor(x => x.Email)
				.NotEmpty()
				.EmailAddress().WithMessage("Invalid email format.");
			RuleFor(x => x.PasswordHash)
				.NotEmpty().WithMessage("Password is required.")
				.MinimumLength(8).WithMessage("Password must be at least 8 characters.")
				.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
				.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
				.Matches("[0-9]").WithMessage("Password must contain at least one number.")
				.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
		}
	}

	public class RegisterTeamMemberRequestDTOValidator : AbstractValidator<RegisterTeamMemberRequestDTO>
	{
		public RegisterTeamMemberRequestDTOValidator()
		{
			RuleFor(x => x.FirstName).NotEmpty();
			RuleFor(x => x.LastName).NotEmpty();
			RuleFor(x => x.UserName).NotEmpty();
			RuleFor(x => x.Email).NotEmpty().EmailAddress();

			RuleFor(x => x.PasswordHash)
				.NotEmpty()
				.MinimumLength(8);
		}
	}

	public class RegisterManagerRequestDTOValidator : AbstractValidator<RegisterManagerRequestDTO>
	{
		public RegisterManagerRequestDTOValidator()
		{
			RuleFor(x => x.FirstName).NotEmpty();
			RuleFor(x => x.LastName).NotEmpty();
			RuleFor(x => x.Email).NotEmpty().EmailAddress();
			RuleFor(x => x.PasswordHash).NotEmpty().MinimumLength(8);
		}
	}

	public class RegisterOrganizationRequestDTOValidator : AbstractValidator<RegisterOrganizationRequestDTO>
	{
		public RegisterOrganizationRequestDTOValidator()
		{
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Organization Name is required.")
				.MinimumLength(2).WithMessage("Organization Name must be at least 2 characters.");
		}
	}
}
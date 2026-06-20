using FluentValidation;
using Application.DTOs;
using Application.Extensions;
using Application.Features.Auth.Commands.RegisterOrganization;

namespace Application.Validators
{
    public class RegisterOrganizationCommandValidator : AbstractValidator<RegisterOrganizationCommand>
    {
        public RegisterOrganizationCommandValidator()
        {
            RuleFor(x => x.Request.OrganizationName)
                .NotEmpty().WithMessage("Organization name is required.");

            RuleFor(x => x.Request.ManagerFirstName)
                .NotEmpty().WithMessage("Manager's first name is required.");

            RuleFor(x => x.Request.ManagerLastName)
                .NotEmpty().WithMessage("Manager's last name is required.");

            RuleFor(x => x.Request.ManagerEmail)
                .NotEmpty().WithMessage("Manager's email is required.")
                .EmailAddress().WithMessage("Manager's email is not a valid email address.");

            RuleFor(x => x.Request.ManagerUserName)
                .NotEmpty().WithMessage("Manager's username is required.");

            RuleFor(x => x.Request.ManagerPassword)
                .NotEmpty().WithMessage("Manager's password is required.")
                .MinimumLength(8).WithMessage("Manager's password must be at least 8 characters long.");

            RuleFor(x => x.Request.OrganizationProfilePicture)
                .IsValidImage();
        }
    }
}

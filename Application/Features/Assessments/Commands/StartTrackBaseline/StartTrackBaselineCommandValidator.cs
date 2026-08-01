using FluentValidation;

namespace Application.Features.Assessments.Commands.StartTrackBaseline
{
    public class StartTrackBaselineCommandValidator : AbstractValidator<StartTrackBaselineCommand>
    {
        public StartTrackBaselineCommandValidator()
        {
            RuleFor(v => v.Dto.CareerPathTrackId)
                .NotEmpty().WithMessage("CareerPathTrackId is required.");

            RuleFor(v => v.Dto.DeclaredProficiencyLevel)
                .IsInEnum().WithMessage("DeclaredProficiencyLevel is invalid.");
        }
    }
}

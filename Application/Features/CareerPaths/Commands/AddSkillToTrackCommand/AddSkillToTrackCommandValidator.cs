using Application.Interfaces.Repository;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.AddSkillToTrackCommand
{
    public class AddSkillToTrackCommandValidator : AbstractValidator<AddSkillToTrackCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddSkillToTrackCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(c => c.CareerPathId)
                .NotEmpty().WithMessage("{PropertyName} is Required.");

            RuleFor(c => c.TrackId)
                .NotEmpty().WithMessage("{PropertyName} is Required.");

            RuleFor(c => c.SkillId)
                .NotEmpty().WithMessage("{PropertyName} is Required.");
        }
    }
}

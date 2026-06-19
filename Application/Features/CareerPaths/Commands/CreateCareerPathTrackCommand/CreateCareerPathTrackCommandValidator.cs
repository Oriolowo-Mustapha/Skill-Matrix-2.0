using Application.Interfaces.Repository;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.CreateCareerPathTrackCommand
{
    public class CreateCareerPathTrackCommandValidator : AbstractValidator<CreateCareerPathTrackCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateCareerPathTrackCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(c => c.CareerPathId)
                .NotEmpty().WithMessage("{PropertyName} is Required.")
                .MustAsync(CareerPathMustExist).WithMessage("CareerPath with ID {PropertyValue} does not exist.");

            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("{PropertyName} is Required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

            RuleFor(c => c)
                .MustAsync(BeUniqueTrackName).WithMessage("A track with this name already exists for this career path.");

            RuleFor(c => c.Description)
                .MaximumLength(500).WithMessage("{PropertyName} must not exceed 500 characters.");
        }

        private async Task<bool> CareerPathMustExist(Guid careerPathId, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CareerPaths.ExistsAsync(cp => cp.Id == careerPathId);
        }

        private async Task<bool> BeUniqueTrackName(CreateCareerPathTrackCommand command, CancellationToken cancellationToken)
        {
            return !await _unitOfWork.CareerPathTracks.ExistsAsync(
                t => t.CareerPathId == command.CareerPathId && t.Name == command.Name);
        }
    }
}

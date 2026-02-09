using Application.Interfaces.Repository;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.AssignCareerPathToLearnerCommand
{
    public class AssignCareerPathToLearnerCommandValidator : AbstractValidator<AssignCareerPathToLearnerCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignCareerPathToLearnerCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(c => c.LearnerId)
                .NotEmpty().WithMessage("{PropertyName} is Required.")
                .MustAsync(LearnerMustExist).WithMessage("Learner with ID {PropertyValue} does not exist.");

            RuleFor(c => c.CareerPathId)
                .NotEmpty().WithMessage("{PropertyName} is Required.")
                .MustAsync(CareerPathMustExist).WithMessage("CareerPath with ID {PropertyValue} does not exist.");

            RuleFor(c => c)
                .MustAsync(BeUniqueAssignment).WithMessage("This CareerPath is already assigned to this Learner.");
        }

        private async Task<bool> LearnerMustExist(Guid learnerId, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Learners.ExistsAsync(l => l.Id == learnerId);
        }

        private async Task<bool> CareerPathMustExist(Guid careerPathId, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CareerPaths.ExistsAsync(cp => cp.Id == careerPathId);
        }

        private async Task<bool> BeUniqueAssignment(AssignCareerPathToLearnerCommand command, CancellationToken cancellationToken)
        {
            return !await _unitOfWork.AssignedCareerPaths.ExistsAsync(
                acp => acp.LearnerId == command.LearnerId && acp.CareerPathId == command.CareerPathId);
        }
    }
}

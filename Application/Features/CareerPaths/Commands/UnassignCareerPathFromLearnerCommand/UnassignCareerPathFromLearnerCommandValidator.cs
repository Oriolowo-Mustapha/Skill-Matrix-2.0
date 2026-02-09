using Application.Interfaces.Repository;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.UnassignCareerPathFromLearnerCommand
{
    public class UnassignCareerPathFromLearnerCommandValidator : AbstractValidator<UnassignCareerPathFromLearnerCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnassignCareerPathFromLearnerCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(c => c.LearnerId)
                .NotEmpty().WithMessage("{PropertyName} is Required.");

            RuleFor(c => c.CareerPathId)
                .NotEmpty().WithMessage("{PropertyName} is Required.");

            RuleFor(c => c)
                .MustAsync(AssignmentMustExist).WithMessage("This CareerPath is not assigned to this Learner.");
        }

        private async Task<bool> AssignmentMustExist(UnassignCareerPathFromLearnerCommand command, CancellationToken cancellationToken)
        {
            return await _unitOfWork.AssignedCareerPaths.ExistsAsync(
                acp => acp.LearnerId == command.LearnerId && acp.CareerPathId == command.CareerPathId);
        }
    }
}

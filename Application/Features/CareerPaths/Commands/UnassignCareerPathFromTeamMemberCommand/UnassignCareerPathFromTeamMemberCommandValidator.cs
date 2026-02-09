using Application.Interfaces.Repository;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.UnassignCareerPathFromTeamMemberCommand
{
    public class UnassignCareerPathFromTeamMemberCommandValidator : AbstractValidator<UnassignCareerPathFromTeamMemberCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnassignCareerPathFromTeamMemberCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(c => c.TeamMemberId)
                .NotEmpty().WithMessage("{PropertyName} is Required.");

            RuleFor(c => c.CareerPathId)
                .NotEmpty().WithMessage("{PropertyName} is Required.");

            RuleFor(c => c)
                .MustAsync(AssignmentMustExist).WithMessage("This CareerPath is not assigned to this TeamMember.");
        }

        private async Task<bool> AssignmentMustExist(UnassignCareerPathFromTeamMemberCommand command, CancellationToken cancellationToken)
        {
            return await _unitOfWork.AssignedCareerPaths.ExistsAsync(
                acp => acp.TeamMemberId == command.TeamMemberId && acp.CareerPathId == command.CareerPathId);
        }
    }
}

using Application.Interfaces.Repository;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.AssignCareerPathToTeamMemberCommand
{
    public class AssignCareerPathToTeamMemberCommandValidator : AbstractValidator<AssignCareerPathToTeamMemberCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignCareerPathToTeamMemberCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(c => c.TeamMemberId)
                .NotEmpty().WithMessage("{PropertyName} is Required.")
                .MustAsync(TeamMemberMustExist).WithMessage("TeamMember with ID {PropertyValue} does not exist.");

            RuleFor(c => c.CareerPathId)
                .NotEmpty().WithMessage("{PropertyName} is Required.")
                .MustAsync(CareerPathMustExist).WithMessage("CareerPath with ID {PropertyValue} does not exist.");

            RuleFor(c => c)
                .MustAsync(BeUniqueAssignment).WithMessage("This CareerPath is already assigned to this TeamMember.");
        }

        private async Task<bool> TeamMemberMustExist(Guid teamMemberId, CancellationToken cancellationToken)
        {
            return await _unitOfWork.TeamMembers.ExistsAsync(tm => tm.Id == teamMemberId);
        }

        private async Task<bool> CareerPathMustExist(Guid careerPathId, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CareerPaths.ExistsAsync(cp => cp.Id == careerPathId);
        }

        private async Task<bool> BeUniqueAssignment(AssignCareerPathToTeamMemberCommand command, CancellationToken cancellationToken)
        {
            return !await _unitOfWork.AssignedCareerPaths.ExistsAsync(
                acp => acp.TeamMemberId == command.TeamMemberId && acp.CareerPathId == command.CareerPathId);
        }
    }
}

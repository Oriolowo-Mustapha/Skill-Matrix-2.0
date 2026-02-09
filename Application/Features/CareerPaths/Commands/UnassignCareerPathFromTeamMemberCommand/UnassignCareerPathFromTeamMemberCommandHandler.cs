using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.UnassignCareerPathFromTeamMemberCommand
{
    public class UnassignCareerPathFromTeamMemberCommandHandler : IRequestHandler<UnassignCareerPathFromTeamMemberCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnassignCareerPathFromTeamMemberCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UnassignCareerPathFromTeamMemberCommand request, CancellationToken cancellationToken)
        {
            var assignedCareerPath = (await _unitOfWork.AssignedCareerPaths
                .FindAsync(acp => acp.TeamMemberId == request.TeamMemberId && acp.CareerPathId == request.CareerPathId))
                .FirstOrDefault();

            if (assignedCareerPath == null)
            {
                // This should ideally be caught by validation, but as a safeguard.
                throw new NotFoundException($"CareerPath with ID {request.CareerPathId} is not assigned to TeamMember with ID {request.TeamMemberId}.");
            }

            await _unitOfWork.AssignedCareerPaths.DeleteAsync(assignedCareerPath);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.UnassignCareerPathFromLearnerCommand
{
    public class UnassignCareerPathFromLearnerCommandHandler : IRequestHandler<UnassignCareerPathFromLearnerCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnassignCareerPathFromLearnerCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UnassignCareerPathFromLearnerCommand request, CancellationToken cancellationToken)
        {
            var assignedCareerPath = (await _unitOfWork.AssignedCareerPaths
                .FindAsync(acp => acp.LearnerId == request.LearnerId && acp.CareerPathId == request.CareerPathId))
                .FirstOrDefault();

            if (assignedCareerPath == null)
            {
                // This should ideally be caught by validation, but as a safeguard.
                throw new NotFoundException($"CareerPath with ID {request.CareerPathId} is not assigned to Learner with ID {request.LearnerId}.");
            }

            await _unitOfWork.AssignedCareerPaths.DeleteAsync(assignedCareerPath);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

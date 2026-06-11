using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.DeleteCareerPathCommand
{
    public class DeleteCareerPathCommandHandler : IRequestHandler<DeleteCareerPathCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCareerPathCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteCareerPathCommand request, CancellationToken cancellationToken)
        {
            var careerPath = await _unitOfWork.CareerPaths
                .GetByIdAsync(request.Id);

            if (careerPath == null)
            {
                throw new NotFoundException($"CareerPath with ID {request.Id} not found.");
            }

            // Check for existing assignments to prevent deletion
            // Note: Per clarification, AssignedCareerPath instances are independent snapshots,
            // but we still prevent deletion of the master CareerPath if it has been used for assignments.
            var hasAssignments = await _unitOfWork.AssignedCareerPaths.ExistsAsync(
                acp => acp.CareerPathId == request.Id);

            if (hasAssignments)
            {
                throw new ConflictException($"CareerPath with ID {request.Id} cannot be deleted as it has active assignments.");
            }

            // Remove associated CareerPathSkills
            var careerPathSkills = await _unitOfWork.CareerPathSkills
                .FindAsync(cps => cps.CareerPathId == request.Id);

            foreach (var careerPathSkill in careerPathSkills)
            {
                await _unitOfWork.CareerPathSkills.DeleteAsync(careerPathSkill);
            }

            await _unitOfWork.CareerPaths.DeleteAsync(careerPath);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

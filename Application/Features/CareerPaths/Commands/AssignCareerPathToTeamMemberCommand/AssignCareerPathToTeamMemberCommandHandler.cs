using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.AssignCareerPathToTeamMemberCommand
{
    public class AssignCareerPathToTeamMemberCommandHandler : IRequestHandler<AssignCareerPathToTeamMemberCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignCareerPathToTeamMemberCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(AssignCareerPathToTeamMemberCommand request, CancellationToken cancellationToken)
        {
            // The validator already ensures TeamMember and CareerPath exist and the assignment is unique.
            // We need to fetch the CareerPath to get its details (Title, Description, IconURL) for the AssignedCareerPath snapshot.
            var careerPath = await _unitOfWork.CareerPaths.GetByIdAsync(request.CareerPathId);

            if (careerPath == null)
            {
                // This should ideally be caught by validation, but as a safeguard.
                throw new NotFoundException($"CareerPath with ID {request.CareerPathId} not found.");
            }

            var assignedCareerPath = new AssignedCareerPath
            {
                TeamMemberId = request.TeamMemberId,
                CareerPathId = request.CareerPathId, // Link to the original CareerPath
                Title = careerPath.Title, // Snapshot the title
                Description = careerPath.Description, // Snapshot the description
                ImageUrl = careerPath.IconURL, // Snapshot the icon URL
                DateAssigned = DateTime.UtcNow
            };

            await _unitOfWork.AssignedCareerPaths.AddAsync(assignedCareerPath);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return assignedCareerPath.Id;
        }
    }
}

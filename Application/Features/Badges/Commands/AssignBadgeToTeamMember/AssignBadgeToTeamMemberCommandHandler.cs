using Application.DTOs;
using Application.Exceptions;
using Application.Features.Badges.Commands.AssignBadgeToTeamMember;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Badges.Commands.AssignBadgeToTeamMember
{
    public class AssignBadgeToTeamMemberCommandHandler : IRequestHandler<AssignBadgeToTeamMemberCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBadgeEligibilityChecker _eligibilityChecker;

        public AssignBadgeToTeamMemberCommandHandler(IUnitOfWork unitOfWork, IBadgeEligibilityChecker eligibilityChecker)
        {
            _unitOfWork = unitOfWork;
            _eligibilityChecker = eligibilityChecker;
        }

        public async Task<Guid> Handle(AssignBadgeToTeamMemberCommand request, CancellationToken cancellationToken)
        {
            // 1. Retrieve Badge and TeamMember entities
            var badge = await _unitOfWork.Badges.GetByIdAsync(request.BadgeId);
            if (badge == null)
            {
                throw new NotFoundException(nameof(Badge), request.BadgeId);
            }

            var teamMember = await _unitOfWork.TeamMembers.GetByIdAsync(request.TeamMemberId);
            if (teamMember == null)
            {
                throw new NotFoundException(nameof(TeamMember), request.TeamMemberId);
            }

            // 2. Check for existing assignment
            var existingAssignment = await _unitOfWork.AssignedBadges.ExistsAsync(
                ab => ab.BadgeId == request.BadgeId && ab.TeamMemberId == request.TeamMemberId);
            if (existingAssignment)
            {
                throw new ConflictException($"Badge '{badge.Name}' is already assigned to Team Member '{teamMember.Id}'.");
            }

            // 3. Criteria Checking
            var isEligible = await _eligibilityChecker.EvaluateEligibilityAsync(request.TeamMemberId, badge.ProficiencyLevel, badge.Criteria);
            if (!isEligible)
            {
                throw new BadRequestException($"Team Member has not achieved the required criteria or proficiency level ('{badge.ProficiencyLevel}') to earn this badge.");
            }

            // 4. Create AssignedBadge entity
            var assignedBadge = new AssignedBadge
            {
                Id = Guid.NewGuid(),
                BadgeId = request.BadgeId,
                TeamMemberId = request.TeamMemberId,
                DateAwarded = DateTime.UtcNow
            };

            // 5. Persist AssignedBadge
            await _unitOfWork.AssignedBadges.AddAsync(assignedBadge);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return assignedBadge.Id;
        }
    }
}

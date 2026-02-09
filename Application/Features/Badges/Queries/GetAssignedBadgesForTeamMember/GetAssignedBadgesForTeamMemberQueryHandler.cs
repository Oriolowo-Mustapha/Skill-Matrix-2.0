using Application.Exceptions;
using Application.Features.Badges.Queries.GetAssignedBadgesForTeamMember;
using Application.Interfaces.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs; // Assuming BadgeDTO exists
using Application.Extensions; // Using custom mapping extensions
using Domain.Entities;

namespace Application.Features.Badges.Queries.GetAssignedBadgesForTeamMember
{
    public class GetAssignedBadgesForTeamMemberQueryHandler : IRequestHandler<GetAssignedBadgesForTeamMemberQuery, List<BadgeDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignedBadgesForTeamMemberQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<BadgeDTO>> Handle(GetAssignedBadgesForTeamMemberQuery request, CancellationToken cancellationToken)
        {
            var teamMember = await _unitOfWork.TeamMembers.GetByIdAsync(request.TeamMemberId);
            if (teamMember == null)
            {
                throw new NotFoundException(nameof(TeamMember), request.TeamMemberId);
            }

            var assignedBadges = (await _unitOfWork.AssignedBadges.GetAllAsync())
                                 .Where(ab => ab.TeamMemberId == request.TeamMemberId)
                                 .ToList();

            var badgeDTOs = new List<BadgeDTO>();
            foreach (var assignedBadge in assignedBadges)
            {
                var badge = await _unitOfWork.Badges.GetByIdAsync(assignedBadge.BadgeId);
                if (badge != null)
                {
                    badgeDTOs.Add(badge.ToBadgeDTO());
                }
            }

            return badgeDTOs;
        }
    }
}

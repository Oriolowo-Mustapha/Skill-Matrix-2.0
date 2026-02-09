using MediatR;
using System;

namespace Application.Features.Badges.Commands.AssignBadgeToTeamMember
{
    public record AssignBadgeToTeamMemberCommand(Guid BadgeId, Guid TeamMemberId) : IRequest<Guid>; // Returns the Id of the AssignedBadge
}
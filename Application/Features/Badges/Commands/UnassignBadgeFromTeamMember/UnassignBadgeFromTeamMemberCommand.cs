using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Badges.Commands.UnassignBadgeFromTeamMember
{
    public record UnassignBadgeFromTeamMemberCommand(Guid BadgeId, Guid TeamMemberId) : IRequest<Unit>;
}
using MediatR;
using System;
using System.Collections.Generic;
using Application.DTOs; // Assuming BadgeDTO exists

namespace Application.Features.Badges.Queries.GetAssignedBadgesForTeamMember
{
    public record GetAssignedBadgesForTeamMemberQuery(Guid TeamMemberId) : IRequest<List<BadgeDTO>>;
}

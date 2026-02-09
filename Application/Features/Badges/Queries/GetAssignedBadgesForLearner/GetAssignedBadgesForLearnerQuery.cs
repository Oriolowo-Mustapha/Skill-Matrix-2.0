using MediatR;
using System;
using System.Collections.Generic;
using Application.DTOs; // Assuming BadgeDTO exists

namespace Application.Features.Badges.Queries.GetAssignedBadgesForLearner
{
    public record GetAssignedBadgesForLearnerQuery(Guid LearnerId) : IRequest<List<BadgeDTO>>;
}

using MediatR;
using System.Collections.Generic;
using Application.DTOs; // Assuming BadgeDTO exists

namespace Application.Features.Badges.Queries.GetAllBadges
{
    public record GetAllBadgesQuery() : IRequest<List<BadgeDTO>>;
}

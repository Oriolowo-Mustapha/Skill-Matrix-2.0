using MediatR;
using System;
using Application.DTOs; // Assuming BadgeDTO exists

namespace Application.Features.Badges.Queries.GetBadgeById
{
    public record GetBadgeByIdQuery(Guid Id) : IRequest<BadgeDTO>;
}

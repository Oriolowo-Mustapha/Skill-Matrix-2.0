using MediatR;
using System;

namespace Application.Features.Badges.Commands.DeleteBadge
{
    public record DeleteBadgeCommand(Guid Id) : IRequest<Unit>;
}
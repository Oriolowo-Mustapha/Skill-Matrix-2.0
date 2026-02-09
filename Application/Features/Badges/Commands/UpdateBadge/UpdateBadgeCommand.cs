using MediatR;
using System;

namespace Application.Features.Badges.Commands.UpdateBadge
{
    public record UpdateBadgeCommand(
        Guid Id,
        string Name,
        string Description,
        string IconUrl,
        string Criteria,
        string ProficiencyLevel) : IRequest<Unit>; // Update operations typically return Unit or the updated entity/DTO
}
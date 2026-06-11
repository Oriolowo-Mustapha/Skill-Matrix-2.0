using Application.DTOs;
﻿using MediatR;

namespace Application.Features.Badges.Commands.CreateBadge
{
	public record CreateBadgeCommand(
        string Name,
        string Description,
        string IconUrl,
        string Criteria,
        string ProficiencyLevel) : IRequest<Guid>;
}

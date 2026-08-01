using Application.DTOs;
using MediatR;
using System;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Badges.Commands.UpdateBadge
{
    public record UpdateBadgeCommand(
        Guid Id,
        string Name,
        string Description,
        IFormFile? Icon,
        string Criteria,
        string ProficiencyLevel) : IRequest<BaseResponse<string>>;
}
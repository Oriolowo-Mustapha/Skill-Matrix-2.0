using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.CareerPaths.Commands.UpdateCareerPathCommand
{
    public record UpdateCareerPathCommand(
        Guid Id,
        string Title,
        string Description,
        string IconURL,
        List<Guid> SkillIds) : IRequest;
}
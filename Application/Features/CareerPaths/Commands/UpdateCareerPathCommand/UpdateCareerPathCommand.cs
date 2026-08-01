using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Application.Features.CareerPaths.Commands.UpdateCareerPathCommand
{
    public record UpdateCareerPathCommand(
        Guid Id,
        string Title,
        string Description,
        IFormFile? Icon,
        List<Guid> SkillIds) : IRequest<BaseResponse<string>>;
}
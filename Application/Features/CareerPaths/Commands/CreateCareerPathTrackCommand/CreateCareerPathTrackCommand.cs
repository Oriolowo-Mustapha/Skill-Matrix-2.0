using Application.DTOs;
using MediatR;
using System;
using Microsoft.AspNetCore.Http;

namespace Application.Features.CareerPaths.Commands.CreateCareerPathTrackCommand
{
    public record CreateCareerPathTrackCommand(
        Guid CareerPathId,
        string Name,
        string Description,
        IFormFile? Icon) : IRequest<BaseResponse<Guid>>;
}

using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.CareerPaths.Commands.CreateCareerPathTrackCommand
{
    public record CreateCareerPathTrackCommand(
        Guid CareerPathId,
        string Name,
        string Description,
        string? IconUrl) : IRequest<BaseResponse<Guid>>;
}

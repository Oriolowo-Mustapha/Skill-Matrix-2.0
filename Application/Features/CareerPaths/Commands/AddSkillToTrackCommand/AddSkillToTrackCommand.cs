using Application.DTOs;
using Domain.Enum;
using MediatR;
using System;

namespace Application.Features.CareerPaths.Commands.AddSkillToTrackCommand
{
    public record AddSkillToTrackCommand(
        Guid CareerPathId,
        Guid TrackId,
        Guid SkillId,
        ProficiencyLevel TargetLevel) : IRequest<BaseResponse<Guid>>;
}

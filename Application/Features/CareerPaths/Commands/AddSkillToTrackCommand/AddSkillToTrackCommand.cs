using MediatR;
using System;

namespace Application.Features.CareerPaths.Commands.AddSkillToTrackCommand
{
    public record AddSkillToTrackCommand(
        Guid CareerPathId,
        Guid TrackId,
        Guid SkillId,
        Domain.Enum.ProficiencyLevel TargetLevel = Domain.Enum.ProficiencyLevel.Novice) : IRequest<Guid>;
}

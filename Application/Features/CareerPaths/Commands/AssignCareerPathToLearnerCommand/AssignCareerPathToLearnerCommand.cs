using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.CareerPaths.Commands.AssignCareerPathToLearnerCommand
{
    public record AssignCareerPathToLearnerCommand(
        Guid LearnerId,
        Guid CareerPathId,
        Guid? TrackId = null) : IRequest<Guid>;
}

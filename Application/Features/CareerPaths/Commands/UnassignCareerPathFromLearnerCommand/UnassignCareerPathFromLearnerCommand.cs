using MediatR;
using System;

namespace Application.Features.CareerPaths.Commands.UnassignCareerPathFromLearnerCommand
{
    public record UnassignCareerPathFromLearnerCommand(
        Guid LearnerId,
        Guid CareerPathId) : IRequest;
}

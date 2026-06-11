using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.CareerPaths.Commands.AssignCareerPathToLearnerCommand
{
    public record AssignCareerPathToLearnerCommand(
        Guid LearnerId,
        Guid CareerPathId) : IRequest<Guid>; // Returns the Id of the new AssignedCareerPath
}

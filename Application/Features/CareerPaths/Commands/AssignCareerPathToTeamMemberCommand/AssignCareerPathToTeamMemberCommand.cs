using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.CareerPaths.Commands.AssignCareerPathToTeamMemberCommand
{
    public record AssignCareerPathToTeamMemberCommand(
        Guid TeamMemberId,
        Guid CareerPathId) : IRequest<Guid>; // Returns the Id of the new AssignedCareerPath
}

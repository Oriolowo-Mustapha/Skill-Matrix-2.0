using MediatR;
using System;

namespace Application.Features.CareerPaths.Commands.UnassignCareerPathFromTeamMemberCommand
{
    public record UnassignCareerPathFromTeamMemberCommand(
        Guid TeamMemberId,
        Guid CareerPathId) : IRequest;
}

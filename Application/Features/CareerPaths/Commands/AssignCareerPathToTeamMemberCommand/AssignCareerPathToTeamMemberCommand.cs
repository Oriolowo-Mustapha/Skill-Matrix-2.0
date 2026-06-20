using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.CareerPaths.Commands.AssignCareerPathToTeamMemberCommand
{
    public record AssignCareerPathToTeamMemberCommand(
        Guid TeamMemberId,
        Guid CareerPathId,
        Guid? TrackId = null) : IRequest<BaseResponse<Guid>>;
}

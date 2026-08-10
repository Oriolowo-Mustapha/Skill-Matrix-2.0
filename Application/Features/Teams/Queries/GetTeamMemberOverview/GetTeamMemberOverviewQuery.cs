using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Teams.Queries.GetTeamMemberOverview
{
	public record GetTeamMemberOverviewQuery(Guid ManagerId, Guid TeamMemberId) : IRequest<BaseResponse<TeamMemberDetailedOverviewDTO>>;
}

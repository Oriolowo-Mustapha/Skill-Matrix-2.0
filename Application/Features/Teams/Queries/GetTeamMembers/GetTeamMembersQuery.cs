using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Teams.Queries.GetTeamMembers
{
	public record GetTeamMembersQuery(Guid ManagerId) : IRequest<List<TeamMemberDTO>>;
}

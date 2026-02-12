using Application.DTOs;
using MediatR;

namespace Application.Features.CareerPaths.Queries.GetAssignedCareerPathsByTeamMember
{
	public record GetAssignedCareerPathsByTeamMemberQuery(Guid TeamMemberId) : IRequest<List<AssignedCareerPathDTO>>;
}
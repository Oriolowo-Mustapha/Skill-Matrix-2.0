using Application.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.RegisterTeamMember
{
	public record CreateTeamMemberCommand(Guid ManagerId, RegisterTeamMemberRequestDTO request) : IRequest<BaseResponse<TeamMemberDTO>>;
}

using Application.DTOs;
using MediatR;

namespace Application.Features.Assessments.Commands.UpdateUserProfile
{
	public record UpdateUserProfileCommand(Guid userId, UpdateUserRequestDTO Dto) : IRequest<UserDTO>;
}

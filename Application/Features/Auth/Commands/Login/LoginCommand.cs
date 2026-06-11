using Application.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.Login
{
	public record LoginCommand(LoginRequestDTO req) : IRequest<BaseResponse<LoginResponseDTO>>;
}

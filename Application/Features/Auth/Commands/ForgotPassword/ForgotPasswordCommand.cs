using Application.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.ForgotPassword
{
	public record ForgotPasswordCommand(string Email) : IRequest<BaseResponse<bool>>;
}
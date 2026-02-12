using MediatR;

namespace Application.Features.Auth.Commands.VerifyEmail
{
	public record VerifyEmailCommand(string Email, string Token) : IRequest<bool>;
}
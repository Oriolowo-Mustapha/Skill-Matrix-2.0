using Application.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.RegisterLearner
{
	public record RegisterLearnerCommand(RegisterLearnerRequestDTO req) : IRequest<BaseResponse<string>>;
}

using Application.DTOs;
using MediatR;

namespace Application.Features.Gamification.Commands.FreezeStreak
{
	public record FreezeStreakCommand(Guid UserId, string UserRole) : IRequest<BaseResponse<bool>>;
}

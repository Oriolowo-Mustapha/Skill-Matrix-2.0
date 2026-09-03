using Application.DTOs;
using MediatR;

namespace Application.Features.Gamification.Commands.RepairStreak
{
	public record RepairStreakCommand(Guid UserId, string UserRole) : IRequest<BaseResponse<RepairStreakResponseDTO>>;
}

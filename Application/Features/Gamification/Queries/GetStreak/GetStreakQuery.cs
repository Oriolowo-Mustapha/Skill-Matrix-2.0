using Application.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Features.Gamification.Queries.GetStreak
{
	public record GetStreakQuery(Guid UserId, string UserRole) : IRequest<BaseResponse<StreakDTO>>;
}

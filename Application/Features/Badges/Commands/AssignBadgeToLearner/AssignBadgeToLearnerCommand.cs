using Application.DTOs;
using MediatR;

namespace Application.Features.Badges.Commands.AssignBadgeToLearner
{
	public record AssignBadgeToLearnerCommand(Guid BadgeId, Guid LearnerId) : IRequest<Guid>;
}
using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Badges.Commands.UnassignBadgeFromLearner
{
    public record UnassignBadgeFromLearnerCommand(Guid BadgeId, Guid LearnerId) : IRequest<BaseResponse<string>>;
}
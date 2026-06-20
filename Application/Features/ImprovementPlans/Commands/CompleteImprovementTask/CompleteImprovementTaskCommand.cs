using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.ImprovementPlans.Commands.CompleteImprovementTask
{
	public record CompleteImprovementTaskCommand(Guid TaskId, Guid UserId, string UserRole) : IRequest<BaseResponse<string>>;
}

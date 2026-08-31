using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.ImprovementPlans.Queries.GetImprovementPlanById
{
	public record GetImprovementPlanByIdQuery(Guid PlanId, Guid UserId, string UserRole) : IRequest<BaseResponse<ImprovementPlanDTO>>;
}

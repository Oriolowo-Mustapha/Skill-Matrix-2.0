using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Assessments.Commands.GenerateStarterPlan
{
	public record GenerateStarterPlanCommand(GenerateStarterPlanRequestDTO Dto, Guid UserId, string UserRole) : IRequest<BaseResponse<ImprovementPlanDTO>>;
}

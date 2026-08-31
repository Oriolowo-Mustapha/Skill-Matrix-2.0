using MediatR;
using Application.DTOs;
using System;
using System.Collections.Generic;

namespace Application.Features.Assessments.Queries.GetImprovementPlans
{
    public record GetImprovementPlansQuery(Guid? UserId = null, string? UserRole = null) : IRequest<List<ImprovementPlanDTO>>;
}

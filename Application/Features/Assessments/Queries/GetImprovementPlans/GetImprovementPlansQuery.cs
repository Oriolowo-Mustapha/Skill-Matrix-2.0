using MediatR;
using Application.DTOs;
using System.Collections.Generic;

namespace Application.Features.Assessments.Queries.GetImprovementPlans
{
    public record GetImprovementPlansQuery : IRequest<List<ImprovementPlanDTO>>;
}

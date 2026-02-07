using MediatR;
using Application.DTOs;
using System.Collections.Generic;

namespace Application.Features.Assessments.Queries.GetAssessments
{
    public record GetAssessmentsQuery : IRequest<List<AssessmentBatchDTO>>;
}

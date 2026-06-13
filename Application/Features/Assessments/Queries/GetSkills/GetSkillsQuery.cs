using MediatR;
using Application.DTOs;
using System.Collections.Generic;

namespace Application.Features.Assessments.Queries.GetSkills
{
    public record GetSkillsQuery : IRequest<BaseResponse<List<SkillDTO>>>;
}

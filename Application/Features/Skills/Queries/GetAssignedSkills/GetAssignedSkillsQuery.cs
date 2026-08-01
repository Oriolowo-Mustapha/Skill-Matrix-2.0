using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Skills.Queries.GetAssignedSkills
{
    public record GetAssignedSkillsQuery(Guid UserId) : IRequest<BaseResponse<List<SkillDTO>>>;
}

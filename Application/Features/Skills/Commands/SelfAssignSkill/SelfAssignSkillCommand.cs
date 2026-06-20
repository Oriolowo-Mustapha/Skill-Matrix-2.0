using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Skills.Commands.SelfAssignSkill
{
	public record SelfAssignSkillCommand(Guid UserId, Guid SkillId) : IRequest<BaseResponse<bool>>;
}

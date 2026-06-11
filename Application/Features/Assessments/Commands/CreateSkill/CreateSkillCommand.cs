using Application.DTOs;
﻿using MediatR;

namespace Application.Features.Assessments.Commands.CreateSkill
{
	public record CreateSkillCommand(string Name, string Category) : IRequest<BaseResponse<string>>;
}

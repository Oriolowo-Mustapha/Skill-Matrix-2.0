using Application.DTOs;
﻿using MediatR;

namespace Application.Features.Assessments.Commands.UpdateSkill
{
	public record UpdateSkillCommand(Guid Id, string Name, string Category) : IRequest<bool>;
}

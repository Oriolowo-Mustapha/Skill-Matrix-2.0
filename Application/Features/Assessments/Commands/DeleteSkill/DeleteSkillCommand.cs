using MediatR;

namespace Application.Features.Assessments.Commands.DeleteSkill
{
	public record DeleteSkillCommand(Guid Id) : IRequest<Unit>;
}

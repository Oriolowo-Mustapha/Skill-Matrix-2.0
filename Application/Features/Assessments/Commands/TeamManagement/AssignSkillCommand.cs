using MediatR;

namespace Application.Features.Assessments.Commands.TeamManagement
{
	public record AssignSkillCommand(Guid ManagerId, Guid TeamMemberId, Guid SkillId) : IRequest<bool>;
}

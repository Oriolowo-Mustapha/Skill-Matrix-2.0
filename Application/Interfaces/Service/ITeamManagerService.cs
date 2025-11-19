using Application.DTOs;

namespace Application.Interfaces.Service
{
	public interface ITeamManagerService
	{
		Task<IEnumerable<TeamMemberPerformanceDTO>> GetTeamMemberPerformanceAsync(Guid ManagerId);
		Task AssignSkillToTeamMemberAsync(Guid teamMemberId, Guid ManagerId);
	}
}

using Domain.Entities;

namespace Application.Interfaces.Repository
{
	public interface IAssignedSkillRepository : IGenericRepository<AssignedSkill>
	{
		Task<AssignedSkill?> GetByUserAndSkillId(Guid userId, Guid skillId);
		Task<IEnumerable<AssignedSkill>> GetSkillsWithHistoryByUserIdAsync(Guid userId);
	}
}

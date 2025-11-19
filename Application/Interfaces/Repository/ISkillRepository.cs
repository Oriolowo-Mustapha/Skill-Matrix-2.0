using Domain.Entities;

namespace Application.Interfaces.Repository
{
	public interface ISkillRepository : IGenericRepository<Skill>
	{
		Task<Skill?> GetByNameAsync(string name);
	}
}

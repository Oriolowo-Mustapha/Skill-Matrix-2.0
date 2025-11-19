using Domain.Entities;

namespace Application.Interfaces.Repository
{
	public interface ITeamMemberRepository : IGenericRepository<TeamMember>
	{
		Task<TeamMember?> GetByEmailAsync(string email);
		Task<TeamMember?> GetByUserNameAsync(string userName);
		Task<IEnumerable<TeamMember>> GetByManagerIdAsync(Guid managerId);
	}
}

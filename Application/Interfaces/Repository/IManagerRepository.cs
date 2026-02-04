using Domain.Entities;

namespace Application.Interfaces.Repository
{
	public interface IManagerRepository : IGenericRepository<Manager>
	{
		Task<Manager?> GetByEmailAsync(string email);
		Task<Manager?> GetByUsernameAsync(string userName);
	}
}

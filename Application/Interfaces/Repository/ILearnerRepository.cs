using Domain.Entities;

namespace Application.Interfaces.Repository
{
	public interface ILearnerRepository : IGenericRepository<Learner>
	{
		Task<Learner?> GetByEmailAsync(string email);
		Task<Learner?> GetByUserName(string userName);
	}
}

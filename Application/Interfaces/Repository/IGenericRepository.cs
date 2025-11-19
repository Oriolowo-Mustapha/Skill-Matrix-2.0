using System.Linq.Expressions;

namespace Application.Interfaces.Repository
{
	public interface IGenericRepository<T> where T : class
	{
		Task<T?> GetByIdAsync(int id);
		Task<T?> GetByIdAsync(Guid id);
		Task<IReadOnlyList<T>> GetAllAsync();
		Task<T> AddAsync();
		Task UpdateAsync();
		Task DeleteAsync();
		Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
	}
}

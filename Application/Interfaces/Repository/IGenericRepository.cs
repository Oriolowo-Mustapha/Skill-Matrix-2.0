using System.Linq.Expressions;

namespace Application.Interfaces.Repository
{
	public interface IGenericRepository<T> where T : class
	{
		Task<T?> GetByIdAsync(Guid id);
		Task<IReadOnlyList<T>> GetAllAsync();
		Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
		Task<T> AddAsync(T entity);
		Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entity);
		Task UpdateAsync(T entity);
		Task DeleteAsync(T entity);
		Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
		Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
	}
}

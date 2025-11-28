using Application.Interfaces.Repository;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Implementation.Servicies
{
	public class GenericRepository<T> : IGenericRepository<T> where T : class
	{
		protected readonly MatrixDbContext _context;
		protected readonly DbSet<T> _dbSet;

		public GenericRepository(MatrixDbContext context)
		{
			_context = context;
			_dbSet = _context.Set<T>();
		}

		public virtual async Task<T?> GetByIdAsync(Guid id)
		{
			return await _dbSet.FindAsync(id);
		}

		public virtual async Task<T?> GetByIdAsync(int id)
		{
			return await _dbSet.FindAsync(id);
		}

		public virtual async Task<IReadOnlyList<T>> GetAllAsync()
		{
			return await _dbSet.ToListAsync();
		}

		public virtual async Task<T> AddAsync(T entity)
		{
			await _dbSet.AddAsync(entity);
			return entity;
		}

		public virtual async Task UpdateAsync(T entity)
		{
			_dbSet.Attach(entity);
			_context.Entry(entity).State = EntityState.Modified;
			await Task.CompletedTask;
		}

		public virtual Task DeleteAsync(T entity)
		{
			if (_context.Entry(entity).State == EntityState.Detached)
			{
				_dbSet.Attach(entity);
			}
			_dbSet.Remove(entity);
			return Task.CompletedTask;
		}

		public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
		{
			return await _dbSet.AnyAsync(predicate);
		}


		public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
		{
			await _dbSet.AddRangeAsync(entities);
			return entities;
		}
	}
}
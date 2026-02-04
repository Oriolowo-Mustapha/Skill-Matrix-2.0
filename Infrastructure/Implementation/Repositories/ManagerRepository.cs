using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Implementation.Servicies;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation.Repositories
{
	public class ManagerRepository : GenericRepository<Manager>, IManagerRepository
	{
		public ManagerRepository(MatrixDbContext context) : base(context)
		{
		}

		public async Task<Manager?> GetByEmailAsync(string email)
		{
			return await _context.Managers.FirstOrDefaultAsync(m => m.Email == email);
		}

		public async Task<Manager?> GetByUsernameAsync(string userName)
		{
			return await _context.Managers.FirstOrDefaultAsync(m => m.UserName == userName);
		}
	}
}

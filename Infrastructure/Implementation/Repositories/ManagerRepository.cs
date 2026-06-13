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
			var normalizedEmail = email.Trim().ToLowerInvariant();
			return await _context.Managers.FirstOrDefaultAsync(m => m.Email.ToLower() == normalizedEmail);
		}

		public async Task<Manager?> GetByUsernameAsync(string userName)
		{
			var normalizedUserName = userName.Trim().ToLowerInvariant();
			return await _context.Managers.FirstOrDefaultAsync(m => m.UserName.ToLower() == normalizedUserName);
		}
	}
}

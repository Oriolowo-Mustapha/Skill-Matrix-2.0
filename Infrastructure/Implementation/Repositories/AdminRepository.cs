using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation.Repositories
{
    public class AdminRepository : GenericRepository<Admin>, IAdminRepository
    {
        public AdminRepository(MatrixDbContext context) : base(context)
        {
        }

        public async Task<Admin?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await _context.Admins.FirstOrDefaultAsync(a => a.Email.ToLower() == normalizedEmail);
        }

        public async Task<Admin?> GetByUserNameAsync(string userName)
        {
            var normalizedUserName = userName.Trim().ToLowerInvariant();
            return await _context.Admins.FirstOrDefaultAsync(a => a.UserName.ToLower() == normalizedUserName);
        }
    }
}

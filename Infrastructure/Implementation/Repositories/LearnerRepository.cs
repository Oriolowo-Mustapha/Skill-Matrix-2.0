using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Implementation.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation.Servicies
{
	public class LearnerRepository : GenericRepository<Learner>, ILearnerRepository
	{
		public LearnerRepository(MatrixDbContext context) : base(context)
		{
		}

		public async Task<Learner?> GetByEmailAsync(string email)
		{
			var normalizedEmail = email.Trim().ToLowerInvariant();
			return await _context.Learners.FirstOrDefaultAsync(l => l.Email.ToLower() == normalizedEmail);
		}

		public async Task<Learner?> GetByUserName(string userName)
		{
			var normalizedUserName = userName.Trim().ToLowerInvariant();
			return await _context.Learners.FirstOrDefaultAsync(l => l.UserName.ToLower() == normalizedUserName);
		}
	}
}

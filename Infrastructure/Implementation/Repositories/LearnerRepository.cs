using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
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
			return await _context.Learners.FirstOrDefaultAsync(l => l.Email == email);
		}

		public async Task<Learner?> GetByUserName(string userName)
		{
			return await _context.Learners.FirstOrDefaultAsync(l => l.UserName == userName);
		}
	}
}

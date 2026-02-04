using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation.Servicies
{
	public class SkillRepository : GenericRepository<Skill>, ISkillRepository
	{
		public SkillRepository(MatrixDbContext context) : base(context)
		{
		}

		public async Task<Skill?> GetByNameAsync(string name)
		{
			return await _context.Skills.FirstOrDefaultAsync(s => s.Name == name);
		}
	}
}

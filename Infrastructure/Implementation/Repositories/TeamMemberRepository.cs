using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Implementation.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation.Servicies
{
	public class TeamMemberRepository : GenericRepository<TeamMember>, ITeamMemberRepository
	{
		public TeamMemberRepository(MatrixDbContext context) : base(context)
		{
		}

		public async Task<TeamMember?> GetByEmailAsync(string email)
		{
			var normalizedEmail = email.Trim().ToLowerInvariant();
			return await _context.TeamMembers.FirstOrDefaultAsync(t => t.Email.ToLower() == normalizedEmail);
		}

		public async Task<IEnumerable<TeamMember>> GetByManagerIdAsync(Guid managerId)
		{
			return await _context.TeamMembers
				.Where(t => t.ManagerId == managerId)
				.ToListAsync();
		}

		public async Task<TeamMember?> GetByUserNameAsync(string userName)
		{
			var normalizedUserName = userName.Trim().ToLowerInvariant();
			return await _context.TeamMembers
				.FirstOrDefaultAsync(t => t.UserName.ToLower() == normalizedUserName);
		}
	}
}

using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation.Servicies
{
	public class AssignedSkillRepository : GenericRepository<AssignedSkill>, IAssignedSkillRepository
	{
		public AssignedSkillRepository(MatrixDbContext context) : base(context)
		{
		}

		public async Task<AssignedSkill?> GetByUserAndSkillId(Guid userId, Guid skillId)
		{
			return await _context.AssignedSkills.FirstOrDefaultAsync(ask => (ask.LearnerId == userId || ask.TeamMemberId == userId) && ask.SkillId == skillId);
		}

		public async Task<IEnumerable<AssignedSkill>> GetSkillsWithHistoryByUserIdAsync(Guid userId)
		{
			return await _context.AssignedSkills
				.Include(ask => ask.Skill)
				.Include(ask => ask.AssessmentResults)
				.Where(ask => ask.LearnerId == userId || ask.TeamMemberId == userId)
				.ToListAsync();
		}
	}
}

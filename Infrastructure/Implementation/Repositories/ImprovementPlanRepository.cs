using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Implementation.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation.Servicies
{
	public class ImprovementPlanRepository : GenericRepository<ImprovementPlan>, IImprovementPlanRepository
	{
		public ImprovementPlanRepository(MatrixDbContext context) : base(context)
		{
		}

		public override async Task<IReadOnlyList<ImprovementPlan>> GetAllAsync()
		{
			return await _context.ImprovementPlans
				.Include(ip => ip.RecommendedResources)
				.Include(ip => ip.Tasks)
				.Include(ip => ip.AssignedSkill)
				.Include(ip => ip.AssessmentResult)
				.OrderByDescending(ip => ip.DateGenerated)
				.ToListAsync();
		}

		public async Task<ImprovementPlan?> GetByAssessmentResultIdAsync(Guid assessmentResultId)
		{
			return await _context.ImprovementPlans
				.Include(ip => ip.RecommendedResources)
				.Include(ip => ip.Tasks)
				.Include(ip => ip.AssignedSkill)
				.Include(ip => ip.AssessmentResult)
				.FirstOrDefaultAsync(ip => ip.AssessmentResultId == assessmentResultId);
		}

		public async Task<ImprovementPlan?> GetPlanWIthResoursesAsync(Guid planId)
		{
			return await _context.ImprovementPlans
				.Include(ip => ip.RecommendedResources)
				.Include(ip => ip.Tasks)
				.Include(ip => ip.AssignedSkill)
				.Include(ip => ip.AssessmentResult)
				.FirstOrDefaultAsync(ip => ip.Id == planId);
		}
	}
}

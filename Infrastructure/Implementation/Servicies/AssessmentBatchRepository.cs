using Application.Interfaces.Repository;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation.Servicies
{
	public class AssessmentBatchRepository : GenericRepository<AssessmentBatch>, IAssessmentBatchRepository
	{
		public AssessmentBatchRepository(MatrixDbContext context) : base(context)
		{
		}

		public async Task<AssessmentBatch?> GetBatchForGrading(int batchId)
		{
			return await _context.AssessmentBatches.Include(ab => ab.Assessments)
				.FirstOrDefaultAsync(ab => ab.Id == batchId);
		}

		public async Task<AssessmentBatch?> GetBatchWithQuestionsAsync(int batchId)
		{
			return await _context.AssessmentBatches
				.Include(ab => ab.Assessments)
				.ThenInclude(a => a.AssessmentOptions)
				.FirstOrDefaultAsync(ab => ab.Id == batchId);
		}
		public async Task<AssessmentBatch?> GetBatchForGradingAsync(int batchId)
		{
			return await _context.AssessmentBatches
				.Include(ab => ab.AssignedSkill)
				.Include(ab => ab.Assessments)
					.ThenInclude(a => a.AssessmentOptions)
				.FirstOrDefaultAsync(ab => ab.Id == batchId);
		}
	}
}

using Domain.Entities;

namespace Application.Interfaces.Repository
{
	public interface IAssessmentBatchRepository : IGenericRepository<AssessmentBatch>
	{
		Task<AssessmentBatch?> GetBatchWithQuestionsAsync(int batchId);
		Task<AssessmentBatch?> GetBatchForGrading(int batchId);
		Task<AssessmentBatch?> GetBatchForGradingAsync(int batchId);
	}
}

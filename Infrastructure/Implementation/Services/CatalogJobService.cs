using Application.DTOs.Ai;
using Application.Interfaces.Service;
using System.Collections.Concurrent;

namespace Infrastructure.Implementation.Services
{
    public class CatalogJobService : ICatalogJobService
    {
        private readonly ConcurrentDictionary<string, CatalogJobStatus> _jobs = new ConcurrentDictionary<string, CatalogJobStatus>();

        public CatalogJobStatus CreateJob()
        {
            var jobId = Guid.NewGuid().ToString();
            var job = new CatalogJobStatus
            {
                JobId = jobId,
                Status = "Processing",
                Message = "AI Catalog generation task queued in background...",
                CreatedAt = DateTime.UtcNow
            };
            _jobs[jobId] = job;
            return job;
        }

        public CatalogJobStatus? GetJob(string jobId)
        {
            _jobs.TryGetValue(jobId, out var job);
            return job;
        }

        public void UpdateJob(string jobId, string status, string message, CatalogGenerationResultDto? result = null)
        {
            if (_jobs.TryGetValue(jobId, out var job))
            {
                job.Status = status;
                job.Message = message;
                if (result != null)
                {
                    job.Result = result;
                }
            }
        }
    }
}

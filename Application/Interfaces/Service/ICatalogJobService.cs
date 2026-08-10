using Application.DTOs.Ai;
using System.Collections.Concurrent;

namespace Application.Interfaces.Service
{
    public class CatalogJobStatus
    {
        public string JobId { get; set; } = string.Empty;
        public string Status { get; set; } = "Processing"; // Processing, Completed, Failed
        public string Message { get; set; } = string.Empty;
        public CatalogGenerationResultDto? Result { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public interface ICatalogJobService
    {
        CatalogJobStatus CreateJob();
        CatalogJobStatus? GetJob(string jobId);
        void UpdateJob(string jobId, string status, string message, CatalogGenerationResultDto? result = null);
    }
}

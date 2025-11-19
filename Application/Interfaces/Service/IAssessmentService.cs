using Application.DTOs;

namespace Application.Interfaces.Service
{
	public interface IAssessmentService
	{
		Task<StartAssesmentResponseDTO> StartAssessmentAsync(AssesmentDTO request, Guid userId);
		Task<AssessmentResultDTO> SubmitAssessmentAsync(SubmitAssessmentRequestDTO request, Guid userId);
		Task<AssessmentResultDTO> GetAssessmentResultAsync(Guid resultId, Guid userId);
	}
}

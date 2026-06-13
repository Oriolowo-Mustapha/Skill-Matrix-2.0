using Application.DTOs.Assessments;

namespace Application.Interfaces.Service
{
	public interface ICodeExecutionService
	{
		Task<CodeExecutionResponseDTO> ExecuteCodeAsync(CodeExecutionRequestDTO request);
	}
}

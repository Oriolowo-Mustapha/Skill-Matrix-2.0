namespace Application.DTOs.Assessments
{
	public class CodeExecutionResponseDTO
	{
		public bool IsSuccess { get; set; }
		public string ConsoleOutput { get; set; } = string.Empty;
		public string ErrorMessage { get; set; } = string.Empty;
	}
}

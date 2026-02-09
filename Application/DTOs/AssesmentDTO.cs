namespace Application.DTOs
{
	public record AssesmentDTO
	{
		public Guid AssignedSkillId { get; set; }
	}

	public record StartAssessmentResponseDTO
	{
		public int AssessmentBatchId { get; set; }
		public List<AssessmentQuestionDTO> Questions { get; set; } = new List<AssessmentQuestionDTO>();
	}

	public record AssessmentQuestionDTO
	{
		public int Id { get; set; }
		public string QuestionText { get; set; } = string.Empty;
		public List<AssessmentOptionDTO> Options { get; set; } = new List<AssessmentOptionDTO>();
	}

	public record AssessmentOptionDTO
	{
		public int id { get; set; }
		public string OptionText { get; set; } = string.Empty;
	}

	public record SubmitAssessmentRequestDTO
	{
		public int AssessmentBatchId { get; set; }
		public List<UserAnswerDTO> UserAnswers { get; set; } = new List<UserAnswerDTO>();
	}

	public record UserAnswerDTO
	{
		public int AssessmentQuestionId { get; set; }
		public int SelectedOptionId { get; set; }
	}
}

namespace Application.DTOs
{
	public record AssesmentDTO
	{
		public Guid AssignedSkillId { get; set; }
	}

	public record StartAssesmentResponseDTO
	{
		public int AssessmentBatchId { get; set; }
		public List<AssessmentQuestionDTO> Questions { get; set; }
	}

	public record AssessmentQuestionDTO
	{
		public int Id { get; set; }
		public string QuestionText { get; set; }
		public List<AssessmentOptionDTO> Options { get; set; }
	}

	public record AssessmentOptionDTO
	{
		public int id { get; set; }
		public string OptionText { get; set; }
	}

	public record SubmitAssessmentRequestDTO
	{
		public int AssessmentBatchId { get; set; }
		public List<UserAnswerDTO> UserAnswers { get; set; }
	}

	public record UserAnswerDTO
	{
		public int AssessmentQuestionId { get; set; }
		public int SelectedOptionId { get; set; }
	}
}

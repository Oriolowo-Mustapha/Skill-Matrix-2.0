namespace Application.DTOs
{
	public record AssesmentDTO
	{
		public Guid AssignedSkillId { get; set; }
	}

	public record AssignSkillRequestDTO
	{
		public Guid TeamMemberId { get; set; }
		public Guid SkillId { get; set; }
	}

	public record StartAssessmentResponseDTO
	{
		public int AssessmentBatchId { get; set; }
		public int TimeLimitMinutes { get; set; }
		public List<AssessmentQuestionDTO> Questions { get; set; } = new List<AssessmentQuestionDTO>();
		public string? WarningMessage { get; set; }
	}

	public record AssessmentQuestionDTO
	{
		public int Id { get; set; }
		public string QuestionText { get; set; } = string.Empty;
		public string QuestionType { get; set; } = "MultipleChoice";
		public List<AssessmentOptionDTO> Options { get; set; } = new List<AssessmentOptionDTO>();
		public string? SampleInput { get; set; }
		public string? ExpectedOutput { get; set; }
		public string? CodeTemplate { get; set; }
		public string Concept { get; set; } = string.Empty;
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
		public int? SelectedOptionId { get; set; }
		public string? SubmittedCode { get; set; }
	}

	public record SelfAssignSkillRequestDTO
	{
		public Guid SkillId { get; set; }
	}
}

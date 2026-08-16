using System;
using System.Collections.Generic;

namespace Application.DTOs
{
	public class AssessmentAttemptStateDTO
	{
		public int AssessmentBatchId { get; set; }
		public Guid AssignedSkillId { get; set; }
		public string SkillName { get; set; } = string.Empty;
		public string ProficiencyLevel { get; set; } = string.Empty;
		public string BatchType { get; set; } = "Initial";
		public string Status { get; set; } = "InProgress";
		public DateTime StartedAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;
		public int SecondsRemaining { get; set; }
		public int LastActiveQuestionIndex { get; set; }
		public int TimeLimitMinutes { get; set; }
		public List<AssessmentQuestionDTO> Questions { get; set; } = new List<AssessmentQuestionDTO>();
		public List<SavedQuestionResponseDTO> SavedResponses { get; set; } = new List<SavedQuestionResponseDTO>();
	}

	public class SavedQuestionResponseDTO
	{
		public int QuestionId { get; set; }
		public int? SelectedOptionId { get; set; }
		public string? SubmittedCode { get; set; }
		public bool IsFlagged { get; set; }
		public DateTime UpdatedAt { get; set; }
	}

	public class SaveQuestionResponseDTO
	{
		public int? SelectedOptionId { get; set; }
		public string? SubmittedCode { get; set; }
		public bool IsFlagged { get; set; }
		public int? CurrentQuestionIndex { get; set; }
		public DateTime? ClientUpdatedAt { get; set; }
	}

	public class SaveQuestionResponseResultDTO
	{
		public bool Success { get; set; }
		public DateTime ServerUpdatedAt { get; set; } = DateTime.UtcNow;
		public int SecondsRemaining { get; set; }
		public bool IsExpired { get; set; }
		public string Message { get; set; } = "Saved";
	}
}

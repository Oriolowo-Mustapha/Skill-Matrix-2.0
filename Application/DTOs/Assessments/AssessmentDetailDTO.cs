using System;
using System.Collections.Generic;

namespace Application.DTOs.Assessments
{
	public class AssessmentDetailDTO
	{
		public Guid ResultId { get; set; }
		public string SkillName { get; set; } = string.Empty;
		public string ProficiencyLevel { get; set; } = string.Empty;
		public int Score { get; set; }
		public int McqScore { get; set; }
		public int CodingScore { get; set; }
		public string VerificationStatus { get; set; } = "PartiallyVerified";
		public string PlacedProficiencyLevel { get; set; } = string.Empty;
		public bool Passed { get; set; }
		public int TotalQuestions { get; set; }
		public int NoOfCorrectAnswers { get; set; }
		public int NoOfWrongAnswers { get; set; }
		public int NoOfUnansweredQuestions { get; set; }
		public DateTime DateCompleted { get; set; }
		public Guid? ImprovementPlanId { get; set; }
		public List<QuestionReviewDTO> Questions { get; set; } = new List<QuestionReviewDTO>();
	}

	public class QuestionReviewDTO
	{
		public int QuestionId { get; set; }
		public string QuestionText { get; set; } = string.Empty;
		public string QuestionType { get; set; } = "MultipleChoice"; // "MultipleChoice" | "Coding"
		public string Concept { get; set; } = string.Empty;
		public bool IsCorrect { get; set; }
		public bool IsAnswered { get; set; }
		public bool IsFlagged { get; set; }

		// MCQ specifics
		public MCQReviewDetailDTO? McqDetail { get; set; }

		// Coding specifics
		public CodingReviewDetailDTO? CodingDetail { get; set; }
	}

	public class MCQReviewDetailDTO
	{
		public int? SelectedOptionId { get; set; }
		public string? SelectedOptionText { get; set; }
		public string CorrectAnswerText { get; set; } = string.Empty;
		public List<OptionReviewDTO> Options { get; set; } = new List<OptionReviewDTO>();
	}

	public class OptionReviewDTO
	{
		public int Id { get; set; }
		public string OptionText { get; set; } = string.Empty;
		public bool IsSelected { get; set; }
		public bool IsCorrectOption { get; set; }
	}

	public class CodingReviewDetailDTO
	{
		public string Language { get; set; } = "csharp";
		public string? SubmittedCode { get; set; }
		public string? SampleInput { get; set; }
		public string? ExpectedOutput { get; set; }
		public string? ConsoleOutput { get; set; }
		public string? FunctionName { get; set; }
		public List<TestCaseReviewResultDTO> TestResults { get; set; } = new List<TestCaseReviewResultDTO>();
	}

	public class TestCaseReviewResultDTO
	{
		public int Index { get; set; }
		public string? Input { get; set; }
		public string? ExpectedOutput { get; set; }
		public string? ActualOutput { get; set; }
		public bool Passed { get; set; }
		public bool IsHidden { get; set; }
	}
}

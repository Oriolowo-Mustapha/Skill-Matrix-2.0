using Application.DTOs;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Application.Extensions
{
	public static class AssessmentMappingExtensions
	{
		public static SkillDTO ToDTO(this AssignedSkill skill)
		{
			return new SkillDTO
			{
				Id = skill.Id,
				SkillId = skill.SkillId,
				Name = skill.Name,
				Category = skill.Category,
				ProficiencyLevel = skill.ProficiencyLevel.ToString(),
				TargetProficiencyLevel = skill.TargetProficiencyLevel.ToString(),
				IsFullyMastered = skill.IsFullyMastered,
				DateAssigned = skill.DateAssigned
			};
		}

		public static SkillDTO ToDTO(this Domain.Entities.Skill skill)
		{
			return new SkillDTO
			{
				Id = skill.Id,
				SkillId = skill.Id,
				Name = skill.Name,
				Category = skill.Category,
			};
		}

		public static List<SkillDTO> ToSkillDTOList(this IEnumerable<Domain.Entities.Skill> skills)
		{
			return skills.Select(s => s.ToDTO()).ToList();
		}


		public static AssessmentOptionDTO ToDTO(this AssessmentOptions option)
		{
			return new AssessmentOptionDTO
			{
				id = option.Id,
				OptionText = option.OptionText
			};
		}

		public static AssessmentQuestionDTO ToDTO(this Assessment assessment)
		{
			List<Application.DTOs.Assessments.TestCaseItem>? testCases = null;
			if (!string.IsNullOrEmpty(assessment.TestCases))
			{
				try
				{
					var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
					testCases = System.Text.Json.JsonSerializer.Deserialize<List<Application.DTOs.Assessments.TestCaseItem>>(assessment.TestCases, jsonOptions);
				}
				catch { /* fallback */ }
			}

			return new AssessmentQuestionDTO
			{
				Id = assessment.Id,
				QuestionText = assessment.Questions,
				QuestionType = assessment.QuestionType.ToString(),
				Options = assessment.AssessmentOptions.Select(o => o.ToDTO()).ToList(),
				SampleInput = assessment.SampleInput,
				ExpectedOutput = assessment.ExpectedOutput,
				CodeTemplate = assessment.CodeTemplate,
				FunctionName = assessment.FunctionName ?? "Solve",
				TestCases = testCases ?? new List<Application.DTOs.Assessments.TestCaseItem>(),
				Concept = assessment.Concept
			};
		}

		public static StartAssessmentResponseDTO ToDTO(this AssessmentBatch batch)
		{
			return new StartAssessmentResponseDTO
			{
				AssessmentBatchId = batch.Id,
				TimeLimitMinutes = batch.TimeLimitMinutes ?? 30,
				StartedAt = batch.StartedAt,
				Questions = batch.Assessments.Select(a => a.ToDTO()).ToList()
			};
		}

		public static UserAnswerDTO ToEntity(this UserResponse answer, int assessmentBatchId, Guid userId)
		{
			return new UserAnswerDTO
			{
				AssessmentQuestionId = answer.AssessmentQuestionId,
				SelectedOptionId = answer.SelectedOptionId
			};
		}

		public static AssessmentBatchDTO ToAssessmentBatchDTO(this AssessmentBatch batch)
		{
			return new AssessmentBatchDTO
			{
				Id = batch.Id,
				SkillId = batch.SkillId,
				LearnerID = batch.LearnerID,
				TeamMemberID = batch.TeamMemberID,
				AssessmentStatus = batch.AssessmentStatus,
				DateCreated = batch.DateCreated,
				Assessments = batch.Assessments?.Select(a => a.ToDTO()).ToList() // Map individual assessments
			};
		}

		public static List<AssessmentBatchDTO> ToAssessmentBatchDTOList(this IEnumerable<AssessmentBatch> batches)
		{
			return batches.Select(b => b.ToAssessmentBatchDTO()).ToList();
		}
	}
}
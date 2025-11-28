using Application.DTOs;
using Domain.Entities;

namespace Application.Extensions
{
	public static class AssessmentMappingExtensions
	{
		public static SkillDTO ToDTO(this AssignedSkill skill)
		{
			return new SkillDTO
			{
				Id = skill.Id,
				Name = skill.Name,
				Category = skill.Category,
				ProficiencyLevel = skill.ProficiencyLevel.ToString(),
				DateAssigned = skill.DateAssigned
			};
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
			return new AssessmentQuestionDTO
			{
				Id = assessment.Id,
				QuestionText = assessment.Questions,
				Options = assessment.AssessmentOptions.Select(o => o.ToDTO()).ToList()
			};
		}

		public static StartAssesmentResponseDTO ToDTO(this AssessmentBatch batch)
		{
			return new StartAssesmentResponseDTO
			{
				AssessmentBatchId = batch.Id,
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
	}
}
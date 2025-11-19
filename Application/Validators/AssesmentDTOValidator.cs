using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
	public class AssesmentDTOValidator : AbstractValidator<AssesmentDTO>
	{
		public AssesmentDTOValidator()
		{
			RuleFor(x => x.AssignedSkillId)
				.NotEmpty().WithMessage("Assigned Skill ID is required.")
				.NotEqual(Guid.Empty).WithMessage("Invalid Skill ID.");
		}
	}

	public class SubmitAssessmentRequestDTOValidator : AbstractValidator<SubmitAssessmentRequestDTO>
	{
		public SubmitAssessmentRequestDTOValidator()
		{
			RuleFor(x => x.AssessmentBatchId)
				.GreaterThan(0).WithMessage("A valid Assessment Batch ID is required.");

			RuleFor(x => x.UserAnswers)
				.NotEmpty().WithMessage("You cannot submit an empty assessment.")
				.Must(answers => answers.Count > 0).WithMessage("At least one answer must be provided.");

			RuleForEach(x => x.UserAnswers).SetValidator(new UserAnswerDTOValidator());
		}
	}

	public class UserAnswerDTOValidator : AbstractValidator<UserAnswerDTO>
	{
		public UserAnswerDTOValidator()
		{
			RuleFor(x => x.AssessmentQuestionId)
				.GreaterThan(0).WithMessage("Assessment Question ID must be valid.");

			RuleFor(x => x.SelectedOptionId)
				.GreaterThan(0).WithMessage("You must select a valid option.");
		}
	}
}
using Application.Interfaces.Repository;
using Application.Extensions;
using FluentValidation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.UpdateCareerPathCommand
{
    public class UpdateCareerPathCommandValidator : AbstractValidator<UpdateCareerPathCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCareerPathCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("{PropertyName} is Required.");

            RuleFor(c => c.Title)
                .NotEmpty().WithMessage("{PropertyName} is Required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.")
                .MustAsync(BeUniqueCareerPathTitle).WithMessage("A career path with this title already exists.");

            RuleFor(c => c.Description)
                .NotEmpty().WithMessage("{PropertyName} is Required.")
                .NotNull()
                .MaximumLength(500).WithMessage("{PropertyName} must not exceed 500 characters.");

            RuleFor(c => c.Icon)
                .IsValidImage();

            RuleForEach(c => c.SkillIds)
                .MustAsync(SkillMustExist).WithMessage("Skill with ID {PropertyValue} does not exist.");
        }

        private async Task<bool> BeUniqueCareerPathTitle(UpdateCareerPathCommand command, string title, CancellationToken cancellationToken)
        {
            // Check if any other career path has this title
            return !await _unitOfWork.CareerPaths.ExistsAsync(cp => cp.Title == title && cp.Id != command.Id);
        }

        private async Task<bool> SkillMustExist(Guid skillId, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Skills.ExistsAsync(s => s.Id == skillId);
        }
    }
}

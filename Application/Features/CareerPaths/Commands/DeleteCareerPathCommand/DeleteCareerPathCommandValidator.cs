using Application.Interfaces.Repository;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.DeleteCareerPathCommand
{
    public class DeleteCareerPathCommandValidator : AbstractValidator<DeleteCareerPathCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCareerPathCommandValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("{PropertyName} is Required.")
                .MustAsync(CareerPathMustExist).WithMessage("CareerPath with ID {PropertyValue} not found.")
                .MustAsync(CareerPathMustNotHaveAssignments).WithMessage("CareerPath cannot be deleted as it has assigned career paths.");
        }

        private async Task<bool> CareerPathMustExist(Guid id, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CareerPaths.ExistsAsync(cp => cp.Id == id);
        }

        private async Task<bool> CareerPathMustNotHaveAssignments(Guid id, CancellationToken cancellationToken)
        {
            // Assuming that AssignedCareerPath does not have a direct foreign key to CareerPath
            // but rather copies its data (Title, Description, IconURL).
            // If the user wants to delete a CareerPath, we should prevent it if any
            // AssignedCareerPath has the same Title/Description/IconURL that would make it
            // logically linked.
            // However, based on the clarification, AssignedCareerPath is independent.
            // So, for this validation, I'll check if there's any *logical* dependency
            // that should prevent deletion.
            // Since there's no direct FK, I'll rely on the assumption that if an AssignedCareerPath
            // was created *from* this CareerPath, and if its Title/Description matches, it
            // should prevent deletion of the "template" CareerPath.
            // This is a complex business rule without a direct FK.

            // Given the independence, I'll assume for now that if an AssignedCareerPath *explicitly*
            // refers to a CareerPath (which isn't the case based on domain entities), that would be
            // the check. Since it doesn't, I will allow deletion for now unless a specific
            // rule is provided.

            // The user stated "assigned career path is just more like the relationship between the users and careerpath does that explains it"
            // This implies the assigned career path is not directly linked to the CareerPath entity by ID.
            // Therefore, a direct check for existing AssignedCareerPaths linked by ID to the CareerPath is not possible with current entities.
            // If there's a need to prevent deletion of a CareerPath if an AssignedCareerPath has similar data,
            // that would require more complex logic (e.g., matching by Title, Description).
            // For now, I'll implement a placeholder check that *would* be relevant if AssignedCareerPath had a CareerPathId.
            // If the user wants to explicitly prevent deletion if a *logically* related AssignedCareerPath exists,
            // they would need to define that matching logic.

            // Let's re-read the context about AssignedCareerPath.
            // It has Title, Description, ImageUrl.
            // If we delete a CareerPath, and an AssignedCareerPath has matching Title, Description, IconURL,
            // then that AssignedCareerPath would be referencing non-existent "source" data.
            // So, I *should* prevent deletion if there is a matching AssignedCareerPath.

            var careerPathToDelete = await _unitOfWork.CareerPaths.GetByIdAsync(id);
            if (careerPathToDelete == null) return true; // Already handled by CareerPathMustExist

            var hasAssignments = await _unitOfWork.AssignedCareerPaths.ExistsAsync(acp =>
                acp.Title == careerPathToDelete.Title &&
                acp.Description == careerPathToDelete.Description &&
                acp.ImageUrl == careerPathToDelete.IconURL // Assuming ImageUrl in AssignedCareerPath maps to IconURL in CareerPath
            );

            return !hasAssignments;
        }
    }
}

using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;

namespace Application.Features.Badges.Commands.AssignBadgeToLearner
{
	public class AssignBadgeToLearnerCommandHandler : IRequestHandler<AssignBadgeToLearnerCommand, Guid>
	{
		private readonly IUnitOfWork _unitOfWork;

		public AssignBadgeToLearnerCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Guid> Handle(AssignBadgeToLearnerCommand request, CancellationToken cancellationToken)
		{
			var badge = await _unitOfWork.Badges.GetByIdAsync(request.BadgeId);
			if (badge == null)
			{
				throw new NotFoundException(nameof(Badge), request.BadgeId);
			}

			var learner = await _unitOfWork.Learners.GetByIdAsync(request.LearnerId);
			if (learner == null)
			{
				throw new NotFoundException(nameof(Learner), request.LearnerId);
			}

			var existingAssignment = await _unitOfWork.AssignedBadges.ExistsAsync(
				ab => ab.BadgeId == request.BadgeId && ab.LearnerID == request.LearnerId);
			if (existingAssignment)
			{
				throw new ConflictException($"Badge '{badge.Name}' is already assigned to Learner '{learner.Id}'.");
			}

			// 3. Criteria Checking (Conceptual)
			// If the Badge has a Criteria string, this implies there are conditions to be met.
			// In a real-world scenario with structured criteria (e.g., required skills, completed assessments),
			// this section would involve querying the learner's progress and evaluating against the badge's criteria.
			// For this implementation, we acknowledge the criteria but assume an external process
			// or manual review ensures they are met before this command is executed.
			if (!string.IsNullOrWhiteSpace(badge.Criteria))
			{
				// Add logging or further checks here if criteria were machine-readable.
				// For now, we proceed assuming the criteria have been considered.
				// Example: Console.WriteLine($"Note: Badge '{badge.Name}' has criteria: '{badge.Criteria}'. Assignment proceeding assuming criteria met.");
			}

			var assignedBadge = new AssignedBadge
			{
				Id = Guid.NewGuid(),
				BadgeId = request.BadgeId,
				LearnerID = request.LearnerId,
				DateAwarded = DateTime.UtcNow
			};

			await _unitOfWork.AssignedBadges.AddAsync(assignedBadge);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return assignedBadge.Id;
		}
	}
}

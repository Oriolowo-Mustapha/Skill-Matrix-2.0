using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.Badges.Commands.AssignBadgeToLearner
{
	public class AssignBadgeToLearnerCommandHandler : IRequestHandler<AssignBadgeToLearnerCommand, BaseResponse<Guid>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IBadgeEligibilityChecker _eligibilityChecker;

		public AssignBadgeToLearnerCommandHandler(IUnitOfWork unitOfWork, IBadgeEligibilityChecker eligibilityChecker)
		{
			_unitOfWork = unitOfWork;
			_eligibilityChecker = eligibilityChecker;
		}

		public async Task<BaseResponse<Guid>> Handle(AssignBadgeToLearnerCommand request, CancellationToken cancellationToken)
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

			// 3. Criteria Checking
			var isEligible = await _eligibilityChecker.EvaluateEligibilityAsync(request.LearnerId, badge.ProficiencyLevel, badge.Criteria);
			if (!isEligible)
			{
				throw new BadRequestException($"Learner has not achieved the required criteria or proficiency level ('{badge.ProficiencyLevel}') to earn this badge.");
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

			return BaseResponse<Guid>.SuccessResponse(assignedBadge.Id, "Badge successfully assigned to learner.");
		}
	}
}

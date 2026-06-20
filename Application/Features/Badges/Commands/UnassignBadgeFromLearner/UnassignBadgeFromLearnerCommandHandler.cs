using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Badges.Commands.UnassignBadgeFromLearner
{
	public class UnassignBadgeFromLearnerCommandHandler : IRequestHandler<UnassignBadgeFromLearnerCommand, BaseResponse<string>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public UnassignBadgeFromLearnerCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<string>> Handle(UnassignBadgeFromLearnerCommand request, CancellationToken cancellationToken)
		{
			var assignedBadge = (await _unitOfWork.AssignedBadges.GetAllAsync())
								.FirstOrDefault(ab => ab.BadgeId == request.BadgeId && ab.LearnerID == request.LearnerId);

			if (assignedBadge == null)
			{
				throw new NotFoundException($"Badge with ID '{request.BadgeId}' is not assigned to Learner with ID '{request.LearnerId}'.");
			}

			await _unitOfWork.AssignedBadges.DeleteAsync(assignedBadge);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return BaseResponse<string>.SuccessResponse(" ", "Badge successfully unassigned from learner.");
		}
	}
}

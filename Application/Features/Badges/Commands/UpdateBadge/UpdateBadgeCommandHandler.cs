using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;

namespace Application.Features.Badges.Commands.UpdateBadge
{
	public class UpdateBadgeCommandHandler : IRequestHandler<UpdateBadgeCommand, BaseResponse<string>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public UpdateBadgeCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<string>> Handle(UpdateBadgeCommand request, CancellationToken cancellationToken)
		{
			var badge = await _unitOfWork.Badges.GetByIdAsync(request.Id);

			if (badge == null)
			{
				throw new NotFoundException(nameof(Badge), request.Id);
			}

			badge.Name = request.Name;
			badge.Description = request.Description;
			badge.IconURL = request.IconUrl;
			badge.Criteria = request.Criteria;
			badge.ProficiencyLevel = request.ProficiencyLevel;

			await _unitOfWork.Badges.UpdateAsync(badge);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return BaseResponse<string>.SuccessResponse(" ", "Badge updated successfully.");
		}
	}
}

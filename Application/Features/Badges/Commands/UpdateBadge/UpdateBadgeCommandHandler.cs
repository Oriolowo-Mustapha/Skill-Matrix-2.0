using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Badges.Commands.UpdateBadge
{
	public class UpdateBadgeCommandHandler : IRequestHandler<UpdateBadgeCommand, BaseResponse<string>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPhotoService _photoService;

		public UpdateBadgeCommandHandler(IUnitOfWork unitOfWork, IPhotoService photoService)
		{
			_unitOfWork = unitOfWork;
			_photoService = photoService;
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
			badge.Criteria = request.Criteria;
			badge.ProficiencyLevel = request.ProficiencyLevel;

			if (request.Icon != null)
			{
				badge.IconURL = await _photoService.AddPhotoAsync(request.Icon);
			}

			await _unitOfWork.Badges.UpdateAsync(badge);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return BaseResponse<string>.SuccessResponse(" ", "Badge updated successfully.");
		}
	}
}

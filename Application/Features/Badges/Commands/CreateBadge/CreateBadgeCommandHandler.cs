using Application.DTOs;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Badges.Commands.CreateBadge
{
	public class CreateBadgeCommandHandler : IRequestHandler<CreateBadgeCommand, BaseResponse<Guid>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPhotoService _photoService;

		public CreateBadgeCommandHandler(IUnitOfWork unitOfWork, IPhotoService photoService)
		{
			_unitOfWork = unitOfWork;
			_photoService = photoService;
		}

		public async Task<BaseResponse<Guid>> Handle(CreateBadgeCommand request, CancellationToken cancellationToken)
		{
			string iconUrl = string.Empty;
			if (request.Icon != null)
			{
				iconUrl = await _photoService.AddPhotoAsync(request.Icon);
			}

			var badge = new Badge
			{
				Id = Guid.NewGuid(),
				Name = request.Name,
				Description = request.Description,
				IconURL = iconUrl,
				Criteria = request.Criteria,
				ProficiencyLevel = request.ProficiencyLevel,
				DateAdded = DateTime.UtcNow
			};

			await _unitOfWork.Badges.AddAsync(badge);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return BaseResponse<Guid>.SuccessResponse(badge.Id, "Badge created successfully.");
		}
	}
}

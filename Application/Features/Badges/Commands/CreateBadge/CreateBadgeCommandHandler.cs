using Application.DTOs;
﻿using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;

namespace Application.Features.Badges.Commands.CreateBadge
{
	public class CreateBadgeCommandHandler : IRequestHandler<CreateBadgeCommand, Guid>
	{
		private readonly IUnitOfWork _unitOfWork;

		public CreateBadgeCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Guid> Handle(CreateBadgeCommand request, CancellationToken cancellationToken)
		{
			var badge = new Badge
			{
				Id = Guid.NewGuid(),
				Name = request.Name,
				Description = request.Description,
				IconURL = request.IconUrl,
				Criteria = request.Criteria,
				ProficiencyLevel = request.ProficiencyLevel,
				DateAdded = DateTime.UtcNow
			};

			await _unitOfWork.Badges.AddAsync(badge);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return badge.Id;
		}
	}
}

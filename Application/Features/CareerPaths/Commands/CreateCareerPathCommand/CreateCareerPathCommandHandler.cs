using Application.DTOs;
﻿using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;

namespace Application.Features.CareerPaths.Commands.CreateCareerPathCommand
{
	public class CreateCareerPathCommandHandler : IRequestHandler<CreateCareerPathCommand, Guid>
	{
		private readonly IUnitOfWork _unitOfWork;

		public CreateCareerPathCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Guid> Handle(CreateCareerPathCommand request, CancellationToken cancellationToken)
		{
			var careerPath = new CareerPath
			{
				Title = request.Title,
				Description = request.Description,
				IconURL = request.IconURL
			};

			await _unitOfWork.CareerPaths.AddAsync(careerPath);

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return careerPath.Id;
		}
	}
}

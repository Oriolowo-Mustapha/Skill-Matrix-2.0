using Application.DTOs;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Commands.CreateCareerPathCommand
{
	public class CreateCareerPathCommandHandler : IRequestHandler<CreateCareerPathCommand, BaseResponse<Guid>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPhotoService _photoService;

		public CreateCareerPathCommandHandler(IUnitOfWork unitOfWork, IPhotoService photoService)
		{
			_unitOfWork = unitOfWork;
			_photoService = photoService;
		}

		public async Task<BaseResponse<Guid>> Handle(CreateCareerPathCommand request, CancellationToken cancellationToken)
		{
			string iconUrl = string.Empty;
			if (request.Icon != null)
			{
				iconUrl = await _photoService.AddPhotoAsync(request.Icon);
			}

			var careerPath = new CareerPath
			{
				Title = request.Title,
				Description = request.Description,
				IconURL = iconUrl
			};

			await _unitOfWork.CareerPaths.AddAsync(careerPath);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return BaseResponse<Guid>.SuccessResponse(careerPath.Id, "Career path created successfully.");
		}
	}
}

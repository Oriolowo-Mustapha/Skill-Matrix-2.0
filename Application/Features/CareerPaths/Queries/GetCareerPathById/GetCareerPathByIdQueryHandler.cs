using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.CareerPaths.Queries.GetCareerPathById
{
	public class GetCareerPathByIdQueryHandler : IRequestHandler<GetCareerPathByIdQuery, CareerPathDTO>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetCareerPathByIdQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<CareerPathDTO> Handle(GetCareerPathByIdQuery request, CancellationToken cancellationToken)
		{
			var careerPaths = await _unitOfWork.CareerPaths.FindAsync(
				cp => cp.Id == request.Id,
				cp => cp.CareerPathSkills,
				cp => cp.Tracks);

			var careerPath = careerPaths.FirstOrDefault();

			if (careerPath == null)
			{
				throw new NotFoundException($"CareerPath with ID {request.Id} not found.");
			}

			await _unitOfWork.CareerPathSkills.FindAsync(
				cps => cps.CareerPathId == request.Id,
				cps => cps.Skill
			);

			return careerPath.ToDto();
		}
	}
}
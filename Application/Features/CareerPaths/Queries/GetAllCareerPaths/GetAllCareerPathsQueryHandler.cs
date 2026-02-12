using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.CareerPaths.Queries.GetAllCareerPaths
{
	public class GetAllCareerPathsQueryHandler : IRequestHandler<GetAllCareerPathsQuery, List<CareerPathDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetAllCareerPathsQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<List<CareerPathDTO>> Handle(GetAllCareerPathsQuery request, CancellationToken cancellationToken)
		{
			var careerPaths = await _unitOfWork.CareerPaths.GetAllAsync(
				cp => cp.CareerPathSkills);

			return careerPaths.ToDtoList();
		}
	}
}
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
				cp => cp.CareerPathSkills,
				cp => cp.Tracks);

			var pathIds = careerPaths.Select(cp => cp.Id).ToList();
			if (pathIds.Any())
			{
				await _unitOfWork.CareerPathSkills.FindAsync(
					cps => pathIds.Contains(cps.CareerPathId),
					cps => cps.Skill
				);
			}

			return careerPaths.OrderBy(cp => cp.Title).ToDtoList();
		}
	}
}
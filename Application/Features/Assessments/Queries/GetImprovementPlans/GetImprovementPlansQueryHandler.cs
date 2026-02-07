using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Assessments.Queries.GetImprovementPlans
{
	public class GetImprovementPlansQueryHandler : IRequestHandler<GetImprovementPlansQuery, List<ImprovementPlanDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetImprovementPlansQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<List<ImprovementPlanDTO>> Handle(GetImprovementPlansQuery request, CancellationToken cancellationToken)
		{
			var improvementPlans = await _unitOfWork.ImprovementPlans.GetAllAsync();
			return improvementPlans.ToImprovementPlanDTOList();
		}
	}
}

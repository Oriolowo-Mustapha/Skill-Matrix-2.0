using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Assessments.Queries.GetAssessments
{
	public class GetAssessmentsQueryHandler : IRequestHandler<GetAssessmentsQuery, List<AssessmentBatchDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetAssessmentsQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<List<AssessmentBatchDTO>> Handle(GetAssessmentsQuery request, CancellationToken cancellationToken)
		{
			var assessmentBatches = await _unitOfWork.AssessmentBatches.GetAllAsync();
			return assessmentBatches.ToAssessmentBatchDTOList();
		}
	}
}

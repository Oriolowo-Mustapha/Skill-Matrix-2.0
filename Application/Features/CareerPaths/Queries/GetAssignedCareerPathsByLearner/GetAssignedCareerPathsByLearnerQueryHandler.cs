using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.CareerPaths.Queries.GetAssignedCareerPathsByLearner
{
	public class GetAssignedCareerPathsByLearnerQueryHandler : IRequestHandler<GetAssignedCareerPathsByLearnerQuery, List<AssignedCareerPathDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public GetAssignedCareerPathsByLearnerQueryHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<List<AssignedCareerPathDTO>> Handle(GetAssignedCareerPathsByLearnerQuery request, CancellationToken cancellationToken)
		{
			var learner = await _unitOfWork.Learners.GetByIdAsync(request.LearnerId);
			if (learner == null)
			{
				throw new NotFoundException($"Learner with ID {request.LearnerId} not found.");
			}

			var assignedCareerPaths = await _unitOfWork.AssignedCareerPaths.FindAsync(
				acp => acp.LearnerId == request.LearnerId,
				acp => acp.CareerPathTrack!);

			return assignedCareerPaths.Select(acp => new AssignedCareerPathDTO
			{
				Id = acp.Id,
				Title = acp.Title,
				Description = acp.Description,
				ImageUrl = acp.ImageUrl,
				CareerPathId = acp.CareerPathId,
				CareerPathTrackId = acp.CareerPathTrackId,
				TrackName = acp.CareerPathTrack?.Name,
				DateAssigned = acp.DateAssigned,
				ProgressPercentage = acp.ProgressPercentage
			}).ToList();
		}
	}
}
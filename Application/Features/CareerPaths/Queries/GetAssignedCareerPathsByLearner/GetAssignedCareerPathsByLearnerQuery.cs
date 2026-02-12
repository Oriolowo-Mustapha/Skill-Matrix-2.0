using Application.DTOs;
using MediatR;

namespace Application.Features.CareerPaths.Queries.GetAssignedCareerPathsByLearner
{
	public record GetAssignedCareerPathsByLearnerQuery(Guid LearnerId) : IRequest<List<AssignedCareerPathDTO>>;
}
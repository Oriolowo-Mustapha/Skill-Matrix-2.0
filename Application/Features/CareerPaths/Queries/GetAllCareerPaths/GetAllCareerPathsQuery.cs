using Application.DTOs;
using MediatR;

namespace Application.Features.CareerPaths.Queries.GetAllCareerPaths
{
	public record GetAllCareerPathsQuery : IRequest<List<CareerPathDTO>>;
}
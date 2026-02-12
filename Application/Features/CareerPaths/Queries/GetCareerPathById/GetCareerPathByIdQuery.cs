using Application.DTOs;
using MediatR;

namespace Application.Features.CareerPaths.Queries.GetCareerPathById
{
	public record GetCareerPathByIdQuery(Guid Id) : IRequest<CareerPathDTO>;
}
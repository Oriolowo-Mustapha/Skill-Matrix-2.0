using Application.DTOs;
using MediatR;

namespace Application.Features.CareerPaths.Queries.GetCareerPathById
{
	public record GetCareerPathByIdQuery(Guid Id, Guid? TrackId = null) : IRequest<CareerPathDTO>;
}
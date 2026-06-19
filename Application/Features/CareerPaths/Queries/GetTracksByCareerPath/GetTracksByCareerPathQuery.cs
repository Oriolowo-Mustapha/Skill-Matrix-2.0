using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.CareerPaths.Queries.GetTracksByCareerPath
{
    public record GetTracksByCareerPathQuery(Guid CareerPathId) : IRequest<List<CareerPathTrackDTO>>;
}

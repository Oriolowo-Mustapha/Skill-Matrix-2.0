using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Dashboard.Queries.GetMyOverview
{
	public record GetMyOverviewQuery(Guid UserId, string UserRole) : IRequest<BaseResponse<MyOverviewDTO>>;
}
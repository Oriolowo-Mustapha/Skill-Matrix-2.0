using Application.DTOs;
using MediatR;

namespace Application.Features.Gamification.Queries.GetXpConfig
{
	public record GetXpConfigQuery : IRequest<BaseResponse<XpConfigDTO>>;
}

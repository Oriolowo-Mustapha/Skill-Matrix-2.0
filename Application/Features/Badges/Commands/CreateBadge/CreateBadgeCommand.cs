using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Badges.Commands.CreateBadge
{
	public record CreateBadgeCommand(
        string Name,
        string Description,
        IFormFile? Icon,
        string Criteria,
        string ProficiencyLevel) : IRequest<BaseResponse<Guid>>;
}

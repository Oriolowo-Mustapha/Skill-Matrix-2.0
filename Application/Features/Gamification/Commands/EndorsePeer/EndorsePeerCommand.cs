using Application.DTOs;
using MediatR;

namespace Application.Features.Gamification.Commands.EndorsePeer
{
	public class EndorsePeerCommand : IRequest<BaseResponse<bool>>
	{
		public Guid EndorserId { get; set; }
		public Guid EndorseeId { get; set; }
		public Guid SkillId { get; set; }
		public string Comment { get; set; } = string.Empty;
	}
}

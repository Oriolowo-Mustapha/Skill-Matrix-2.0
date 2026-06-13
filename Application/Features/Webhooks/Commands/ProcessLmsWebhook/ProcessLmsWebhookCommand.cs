using Application.DTOs;
using Application.DTOs.Webhooks;
using MediatR;

namespace Application.Features.Webhooks.Commands.ProcessLmsWebhook
{
	public class ProcessLmsWebhookCommand : IRequest<BaseResponse<bool>>
	{
		public LmsCourseCompletedWebhookDTO Payload { get; set; } = new LmsCourseCompletedWebhookDTO();
	}
}

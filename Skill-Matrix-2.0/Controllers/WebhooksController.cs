using Application.DTOs;
using Application.DTOs.Webhooks;
using Application.Features.Webhooks.Commands.ProcessLmsWebhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Skill_Matrix_2._0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class WebhooksController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly IConfiguration _configuration;

		public WebhooksController(IMediator mediator, IConfiguration configuration)
		{
			_mediator = mediator;
			_configuration = configuration;
		}

		[HttpPost("lms-course-completed")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<bool>>> LmsCourseCompleted([FromBody] LmsCourseCompletedWebhookDTO payload)
		{
			// Basic security: require an API Key header "X-Webhook-Secret"
			if (!Request.Headers.TryGetValue("X-Webhook-Secret", out var extractedSecret))
			{
				return Unauthorized("Webhook Secret is missing.");
			}

			var configuredSecret = _configuration["Webhooks:LmsSecret"];
			if (string.IsNullOrEmpty(configuredSecret) || extractedSecret != configuredSecret)
			{
				return Unauthorized("Invalid Webhook Secret.");
			}

			var command = new ProcessLmsWebhookCommand { Payload = payload };
			var response = await _mediator.Send(command);

			return Ok(response);
		}
	}
}

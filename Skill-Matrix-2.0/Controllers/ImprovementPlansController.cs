using Application.DTOs;

using Application.Features.Assessments.Queries.GetImprovementPlans;
using Application.Features.ImprovementPlans.Commands.GenerateAiImprovementPlan;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Skill_Matrix_2._0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class ImprovementPlansController : ControllerBase
	{
		private readonly IMediator _mediator;

		public ImprovementPlansController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		public async Task<ActionResult<List<ImprovementPlanDTO>>> GetImprovementPlans()
		{
			var query = new GetImprovementPlansQuery();
			var response = await _mediator.Send(query);
			return Ok(response);
		}



		[HttpPost("generate-ai")]
		public async Task<ActionResult<BaseResponse<AIImprovementPlanResponseDTO>>> GenerateAiImprovementPlan([FromBody] GenerateAiImprovementPlanCommand command)
		{
			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpPost("tasks/{taskId}/complete")]
		public async Task<ActionResult<bool>> CompleteTask(Guid taskId)
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userRole = User.FindFirstValue(ClaimTypes.Role);
			if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(userRole))
			{
				return Unauthorized();
			}

			var userId = Guid.Parse(userIdString);
			var command = new Application.Features.ImprovementPlans.Commands.CompleteImprovementTask.CompleteImprovementTaskCommand(taskId, userId, userRole);
			var result = await _mediator.Send(command);
			return Ok(result);
		}
	}
}

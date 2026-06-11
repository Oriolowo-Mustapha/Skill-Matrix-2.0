using Application.DTOs;
using Application.Features.Assessments.Commands.ImprovementPlans;
using Application.Features.Assessments.Queries.GetImprovementPlans;
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
			await _mediator.Send(query);
			return NoContent();
		}

		[HttpPost("generate/{assessmentResultId}")]
		public async Task<ActionResult<ImprovementPlanDTO>> GenerateImprovementPlan(Guid assessmentResultId)
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
			{
				return Unauthorized("Invalid user token.");
			}

			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "Learner";

			var command = new GenerateImprovementPlanCommand(assessmentResultId, userId, userRole);
			await _mediator.Send(command);
			return NoContent();
		}
	}
}

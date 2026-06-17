using Application.DTOs;
using Application.Features.CareerPaths.Commands.CreateCareerPathCommand;
using Application.Features.CareerPaths.Commands.DeleteCareerPathCommand;
using Application.Features.CareerPaths.Commands.UpdateCareerPathCommand;
using Application.Features.CareerPaths.Commands.AssignCareerPathToLearnerCommand;
using Application.Features.CareerPaths.Commands.AssignCareerPathToTeamMemberCommand;
using Application.Features.CareerPaths.Commands.UnassignCareerPathFromLearnerCommand;
using Application.Features.CareerPaths.Commands.UnassignCareerPathFromTeamMemberCommand;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Skill_Matrix_2._0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class CareerPathsController : ControllerBase
	{
		private readonly IMediator _mediator;

		public CareerPathsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<ActionResult<Guid>> CreateCareerPath([FromBody] CreateCareerPathCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<IActionResult> UpdateCareerPath(Guid id, [FromBody] UpdateCareerPathCommand command)
		{
			if (id != command.Id)
				return BadRequest("Route ID and Command ID must match.");

			await _mediator.Send(command);
			return NoContent();
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		public async Task<IActionResult> DeleteCareerPath(Guid id)
		{
			var command = new DeleteCareerPathCommand(id);
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPost("assign-learner")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<IActionResult> AssignToLearner([FromBody] AssignCareerPathToLearnerCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPost("assign-team-member")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<IActionResult> AssignToTeamMember([FromBody] AssignCareerPathToTeamMemberCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPost("unassign-learner")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<IActionResult> UnassignFromLearner([FromBody] UnassignCareerPathFromLearnerCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPost("unassign-team-member")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<IActionResult> UnassignFromTeamMember([FromBody] UnassignCareerPathFromTeamMemberCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}
	}
}

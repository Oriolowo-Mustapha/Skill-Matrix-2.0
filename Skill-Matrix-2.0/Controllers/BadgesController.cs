using Application.DTOs;
using Application.Features.Badges.Commands.CreateBadge;
using Application.Features.Badges.Commands.DeleteBadge;
using Application.Features.Badges.Commands.UpdateBadge;
using Application.Features.Badges.Commands.AssignBadgeToLearner;
using Application.Features.Badges.Commands.AssignBadgeToTeamMember;
using Application.Features.Badges.Commands.UnassignBadgeFromLearner;
using Application.Features.Badges.Commands.UnassignBadgeFromTeamMember;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Skill_Matrix_2._0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class BadgesController : ControllerBase
	{
		private readonly IMediator _mediator;

		public BadgesController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		[Authorize(Roles = "Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<ActionResult<Guid>> CreateBadge([FromBody] CreateBadgeCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<IActionResult> UpdateBadge(Guid id, [FromBody] UpdateBadgeCommand command)
		{
			if (id != command.Id)
				return BadRequest("Route ID and Command ID must match.");

			await _mediator.Send(command);
			return NoContent();
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin,SuperAdmin")]
		public async Task<IActionResult> DeleteBadge(Guid id)
		{
			var command = new DeleteBadgeCommand(id);
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPost("assign-learner")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<IActionResult> AssignToLearner([FromBody] AssignBadgeToLearnerCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPost("assign-team-member")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<IActionResult> AssignToTeamMember([FromBody] AssignBadgeToTeamMemberCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPost("unassign-learner")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<IActionResult> UnassignFromLearner([FromBody] UnassignBadgeFromLearnerCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPost("unassign-team-member")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<IActionResult> UnassignFromTeamMember([FromBody] UnassignBadgeFromTeamMemberCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}
	}
}

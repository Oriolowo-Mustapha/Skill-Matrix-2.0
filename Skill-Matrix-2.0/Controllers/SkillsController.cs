using Application.DTOs;
using Application.Features.Assessments.Commands.CreateSkill;
using Application.Features.Assessments.Commands.DeleteSkill;
using Application.Features.Assessments.Commands.UpdateSkill;
using Application.Features.Assessments.Commands.TeamManagement;
using Application.Features.Assessments.Queries.GetSkills;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Skill_Matrix_2._0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class SkillsController : ControllerBase
	{
		private readonly IMediator _mediator;

		public SkillsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		public async Task<ActionResult<List<SkillDTO>>> GetAllSkills()
		{
			var query = new GetSkillsQuery();
			await _mediator.Send(query);
			return NoContent();
		}

		[HttpPost]
		[Authorize(Roles = "Admin, SuperAdmin")]
		public async Task<ActionResult<Guid>> CreateSkill([FromBody] CreateSkillCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPut("{id}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> UpdateSkill(Guid id, [FromBody] UpdateSkillCommand command)
		{
			if (id != command.Id)
				return BadRequest("Route ID and Command ID must match.");

			await _mediator.Send(command);
			return NoContent();
		}

		[HttpDelete("{id}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> DeleteSkill(Guid id)
		{
			var command = new DeleteSkillCommand(id);
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpPost("assign")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AssignSkill([FromBody] AssignSkillCommand command)
		{
			var managerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(managerIdString) || !Guid.TryParse(managerIdString, out Guid managerId))
			{
				return Unauthorized("Invalid user token.");
			}

			var finalCommand = new AssignSkillCommand(managerId, command.TeamMemberId, command.SkillId);
			
			await _mediator.Send(finalCommand);
			return NoContent();
		}
	}
}

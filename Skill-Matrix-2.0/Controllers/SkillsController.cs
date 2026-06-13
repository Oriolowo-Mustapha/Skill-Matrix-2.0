using Application.DTOs;
using Application.Features.Assessments.Commands.CreateSkill;
using Application.Features.Assessments.Commands.DeleteSkill;
using Application.Features.Assessments.Commands.UpdateSkill;
using Application.Features.Assessments.Commands.TeamManagement;
using Application.Features.Assessments.Queries.GetSkills;
using Application.Features.Skills.Commands;
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
		public async Task<ActionResult<BaseResponse<List<SkillDTO>>>> GetAllSkills()
		{
			var query = new GetSkillsQuery();
			var response = await _mediator.Send(query);
			return Ok(response);
		}

		[HttpPost]
		[Authorize(Roles = "Admin, SuperAdmin")]
		public async Task<ActionResult<BaseResponse<string>>> CreateSkill([FromBody] CreateSkillCommand command)
		{
			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpPut("{id}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> UpdateSkill(Guid id, [FromBody] UpdateSkillCommand command)
		{
			if (id != command.Id)
				return BadRequest("Route ID and Command ID must match.");

			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpDelete("{id}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<IActionResult> DeleteSkill(Guid id)
		{
			var command = new DeleteSkillCommand(id);
			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpPost("sync-lightcast")]
		[Authorize(Roles = "Admin, SuperAdmin")]
		public async Task<ActionResult<BaseResponse<string>>> SyncLightcastSkills([FromQuery] int limit = 500, [FromQuery] string version = "latest")
		{
			var command = new SyncLightcastSkillsCommand { Limit = limit, TaxonomyVersion = version };
			var response = await _mediator.Send(command);
			return Ok(response);
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
			
			var response = await _mediator.Send(finalCommand);
			return Ok(response);
		}
	}
}

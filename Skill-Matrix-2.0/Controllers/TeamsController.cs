using Application.DTOs;
using Application.Features.Auth.Commands.RegisterTeamMember;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Skill_Matrix_2._0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "Manager,Admin,SuperAdmin")]
	public class TeamsController : ControllerBase
	{
		private readonly IMediator _mediator;

		public TeamsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("register-member")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<TeamMemberDTO>>> RegisterTeamMember([FromBody] RegisterTeamMemberRequestDTO request)
		{
			var managerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(managerIdString) || !Guid.TryParse(managerIdString, out Guid managerId))
			{
				return Unauthorized("Invalid user token.");
			}

			var command = new CreateTeamMemberCommand(managerId, request);
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpGet("members")]
		public async Task<ActionResult<BaseResponse<List<TeamMemberDTO>>>> GetMyTeamMembers()
		{
			var managerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(managerIdString) || !Guid.TryParse(managerIdString, out Guid managerId))
			{
				return Unauthorized("Invalid user token.");
			}

			var query = new Application.Features.Teams.Queries.GetTeamMembers.GetTeamMembersQuery(managerId);
			var response = await _mediator.Send(query);
			return Ok(BaseResponse<List<TeamMemberDTO>>.SuccessResponse(response, "Team members retrieved successfully."));
		}
	}
}

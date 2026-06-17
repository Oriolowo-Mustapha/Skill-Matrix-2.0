using Application.DTOs;
using Application.Features.Gamification.Commands.EndorsePeer;
using Application.Features.Gamification.Queries.GetLeaderboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Skill_Matrix_2._0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class GamificationController : ControllerBase
	{
		private readonly IMediator _mediator;

		public GamificationController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("endorse")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<bool>>> EndorsePeer([FromBody] EndorsePeerCommand command)
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
			{
				return Unauthorized("Invalid user token.");
			}

			// Ensure the endorser is the one making the request
			command.EndorserId = userId;

			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpGet("leaderboard/{organizationId}")]
		public async Task<ActionResult<BaseResponse<List<LeaderboardEntryDTO>>>> GetLeaderboard(Guid organizationId)
		{
			var query = new GetLeaderboardQuery { OrganizationId = organizationId };
			var response = await _mediator.Send(query);
			return Ok(response);
		}
	}
}

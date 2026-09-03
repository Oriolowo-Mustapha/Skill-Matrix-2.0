using Application.Features.Dashboard.Queries.GetMyOverview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Skill_Matrix_2_0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class DashboardController : ControllerBase
	{
		private readonly IMediator _mediator;

		public DashboardController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet("overview")]
		public async Task<IActionResult> GetOverview()
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var query = new GetMyOverviewQuery(userId, userRole);
			var response = await _mediator.Send(query);
			return Ok(response);
		}
	}
}
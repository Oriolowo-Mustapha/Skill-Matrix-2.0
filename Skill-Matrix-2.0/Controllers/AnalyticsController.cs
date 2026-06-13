using Application.DTOs;
using Application.DTOs.Analytics;
using Application.Features.Analytics.Queries.GetOrganizationAnalytics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Skill_Matrix_2._0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class AnalyticsController : ControllerBase
	{
		private readonly IMediator _mediator;

		public AnalyticsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet("organization/{organizationId}")]
		[Authorize(Roles = "Manager, Admin, SuperAdmin")]
		public async Task<ActionResult<BaseResponse<OrganizationAnalyticsDTO>>> GetOrganizationAnalytics(Guid organizationId)
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
			{
				return Unauthorized("Invalid user token.");
			}

			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var query = new GetOrganizationAnalyticsQuery
			{
				OrganizationId = organizationId,
				RequesterId = userId,
				RequesterRole = userRole
			};

			var response = await _mediator.Send(query);
			return Ok(response);
		}
	}
}

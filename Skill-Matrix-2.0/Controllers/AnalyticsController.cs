using Application.DTOs;
using Application.DTOs.Analytics;
using Application.Features.Analytics.Queries.GetOrganizationAnalytics;
using Application.Interfaces.Repository;
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
		private readonly IUnitOfWork _unitOfWork;

		public AnalyticsController(IMediator mediator, IUnitOfWork unitOfWork)
		{
			_mediator = mediator;
			_unitOfWork = unitOfWork;
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

		[HttpGet("my-organization")]
		[Authorize(Roles = "Manager")]
		public async Task<ActionResult<BaseResponse<OrganizationAnalyticsDTO>>> GetMyOrganizationAnalytics()
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
			{
				return Unauthorized("Invalid user token.");
			}

			var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(userId);
			if (manager == null) 
			{
				return Unauthorized("Manager profile not found.");
			}

			var query = new GetOrganizationAnalyticsQuery
			{
				OrganizationId = manager.OrganizationId,
				RequesterId = userId,
				RequesterRole = "Manager"
			};

			var response = await _mediator.Send(query);
			return Ok(response);
		}
	}
}

using Application.DTOs;
using Application.Features.Badges.Commands.CreateBadge;
using Application.Features.Badges.Commands.DeleteBadge;
using Application.Features.Badges.Commands.UpdateBadge;
using Application.Features.Badges.Commands.AssignBadgeToLearner;
using Application.Features.Badges.Commands.AssignBadgeToTeamMember;
using Application.Features.Badges.Commands.UnassignBadgeFromLearner;
using Application.Features.Badges.Commands.UnassignBadgeFromTeamMember;
using Application.Features.Badges.Queries.GetAllBadges;
using Application.Features.Badges.Queries.GetBadgeById;
using Application.Features.Badges.Queries.GetAssignedBadgesForLearner;
using Application.Features.Badges.Queries.GetAssignedBadgesForTeamMember;
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

		[HttpGet]
		public async Task<ActionResult<BaseResponse<List<BadgeDTO>>>> GetAll()
		{
			var result = await _mediator.Send(new GetAllBadgesQuery());
			return Ok(BaseResponse<List<BadgeDTO>>.SuccessResponse(result, "Badges retrieved successfully."));
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<BadgeDTO>>> GetById(Guid id)
		{
			var result = await _mediator.Send(new GetBadgeByIdQuery(id));
			return Ok(BaseResponse<BadgeDTO>.SuccessResponse(result, "Badge retrieved successfully."));
		}

		[HttpGet("assigned/learner/{learnerId}")]
		public async Task<ActionResult<BaseResponse<List<BadgeDTO>>>> GetAssignedByLearner(Guid learnerId)
		{
			var result = await _mediator.Send(new GetAssignedBadgesForLearnerQuery(learnerId));
			return Ok(BaseResponse<List<BadgeDTO>>.SuccessResponse(result, "Assigned badges retrieved successfully."));
		}

		[HttpGet("assigned/team-member/{teamMemberId}")]
		public async Task<ActionResult<BaseResponse<List<BadgeDTO>>>> GetAssignedByTeamMember(Guid teamMemberId)
		{
			var result = await _mediator.Send(new GetAssignedBadgesForTeamMemberQuery(teamMemberId));
			return Ok(BaseResponse<List<BadgeDTO>>.SuccessResponse(result, "Assigned badges retrieved successfully."));
		}

		[HttpPost]
		[Authorize(Roles = "Admin,SuperAdmin")]
		[Consumes("multipart/form-data")]
		public async Task<ActionResult<BaseResponse<Guid>>> CreateBadge([FromForm] CreateBadgeCommand command)
		{
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Admin,SuperAdmin")]
		[Consumes("multipart/form-data")]
		public async Task<ActionResult<BaseResponse<string>>> UpdateBadge(Guid id, [FromForm] UpdateBadgeCommand command)
		{
			if (id != command.Id)
				return BadRequest("Route ID and Command ID must match.");

			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin,SuperAdmin")]
		public async Task<ActionResult<BaseResponse<string>>> DeleteBadge(Guid id)
		{
			var command = new DeleteBadgeCommand(id);
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPost("assign-learner")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<Guid>>> AssignToLearner([FromBody] AssignBadgeToLearnerCommand command)
		{
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPost("assign-team-member")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<Guid>>> AssignToTeamMember([FromBody] AssignBadgeToTeamMemberCommand command)
		{
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPost("unassign-learner")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<string>>> UnassignFromLearner([FromBody] UnassignBadgeFromLearnerCommand command)
		{
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPost("unassign-team-member")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<string>>> UnassignFromTeamMember([FromBody] UnassignBadgeFromTeamMemberCommand command)
		{
			var result = await _mediator.Send(command);
			return Ok(result);
		}
	}
}

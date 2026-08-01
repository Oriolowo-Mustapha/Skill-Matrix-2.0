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
		private readonly Application.Interfaces.Repository.IUnitOfWork _unitOfWork;

		public TeamsController(IMediator mediator, Application.Interfaces.Repository.IUnitOfWork unitOfWork)
		{
			_mediator = mediator;
			_unitOfWork = unitOfWork;
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

		[HttpGet("members/{id}/profile")]
		public async Task<ActionResult<BaseResponse<TeamMemberProfileDTO>>> GetTeamMemberProfile(Guid id)
		{
			var managerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(managerIdString) || !Guid.TryParse(managerIdString, out Guid managerId))
			{
				return Unauthorized("Invalid user token.");
			}

			var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(managerId);
			if (manager == null) return Unauthorized("Manager not found.");

			var memberList = await _unitOfWork.TeamMembers.FindAsync(
				m => m.Id == id && m.OrganizationId == manager.OrganizationId,
				m => m.TeamMemberSkills,
				m => m.CareerPaths
			);

			var member = memberList.FirstOrDefault();
			if (member == null) return NotFound("Team member not found.");

			// Fetch exact details of career paths if needed, or map just basic info
			var profile = new TeamMemberProfileDTO
			{
				Id = member.Id,
				FirstName = member.FirstName,
				LastName = member.LastName,
				Email = member.Email,
				Role = member.Role,
				AssignedSkills = member.TeamMemberSkills?.Select(ts => new SkillDTO
				{
					SkillId = ts.SkillId,
					Name = ts.Name,
					Category = ts.Category,
					ProficiencyLevel = ts.ProficiencyLevel.ToString(),
					IsFullyMastered = ts.ProficiencyLevel == Domain.Enum.ProficiencyLevel.Expert,
					DateAssigned = ts.DateAssigned
				}).ToList() ?? new List<SkillDTO>(),
				AssignedPaths = member.CareerPaths?.Select(cp => new CareerPathDTO
				{
					Id = cp.CareerPathId,
					Title = cp.Title,
					Description = cp.Description,
					IconURL = cp.ImageUrl,
					DateAdded = cp.DateAssigned
				}).ToList() ?? new List<CareerPathDTO>()
			};

			return Ok(BaseResponse<TeamMemberProfileDTO>.SuccessResponse(profile, "Profile retrieved successfully."));
		}

		[HttpDelete("members/{id}")]
		public async Task<ActionResult<BaseResponse<string>>> DeleteTeamMember(Guid id)
		{
			var managerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(managerIdString) || !Guid.TryParse(managerIdString, out Guid managerId))
			{
				return Unauthorized("Invalid user token.");
			}

			var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(managerId);
			if (manager == null)
			{
				return Unauthorized("Manager profile not found.");
			}

			var member = await _unitOfWork.TeamMembers.GetByIdAsync(id);
			if (member == null)
			{
				return NotFound("Team member not found.");
			}

			if (member.OrganizationId != manager.OrganizationId)
			{
				return Forbid("You do not have permission to delete a team member from another organization.");
			}

			await _unitOfWork.TeamMembers.DeleteAsync(member);
			await _unitOfWork.SaveChangesAsync(default);

			return Ok(BaseResponse<string>.SuccessResponse("Team member deleted successfully.", "Team member deleted successfully."));
		}
	}
}

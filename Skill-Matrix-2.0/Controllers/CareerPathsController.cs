using Application.DTOs;
using Application.Features.CareerPaths.Commands.AddSkillToTrackCommand;
using Application.Features.CareerPaths.Commands.CreateCareerPathCommand;
using Application.Features.CareerPaths.Commands.CreateCareerPathTrackCommand;
using Application.Features.CareerPaths.Commands.DeleteCareerPathCommand;
using Application.Features.CareerPaths.Commands.UpdateCareerPathCommand;
using Application.Features.CareerPaths.Commands.AssignCareerPathToLearnerCommand;
using Application.Features.CareerPaths.Commands.AssignCareerPathToTeamMemberCommand;
using Application.Features.CareerPaths.Commands.UnassignCareerPathFromLearnerCommand;
using Application.Features.CareerPaths.Commands.UnassignCareerPathFromTeamMemberCommand;
using Application.Features.CareerPaths.Queries.GetAllCareerPaths;
using Application.Features.CareerPaths.Queries.GetCareerPathById;
using Application.Features.CareerPaths.Queries.GetAssignedCareerPathsByLearner;
using Application.Features.CareerPaths.Queries.GetAssignedCareerPathsByTeamMember;
using Application.Features.CareerPaths.Queries.GetTracksByCareerPath;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skill_Matrix_2._0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class CareerPathsController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly Application.Interfaces.Repository.IUnitOfWork _unitOfWork;

		public CareerPathsController(IMediator mediator, Application.Interfaces.Repository.IUnitOfWork unitOfWork)
		{
			_mediator = mediator;
			_unitOfWork = unitOfWork;
		}

		[HttpGet]
		public async Task<ActionResult<BaseResponse<List<CareerPathDTO>>>> GetAll()
		{
			var result = await _mediator.Send(new GetAllCareerPathsQuery());
			return Ok(BaseResponse<List<CareerPathDTO>>.SuccessResponse(result, "Career paths retrieved successfully."));
		}

		[HttpPost("ai-generate-catalog")]
		public async Task<ActionResult<BaseResponse<Application.DTOs.Ai.CatalogGenerationResultDto>>> GenerateAiCatalog()
		{
			var result = await _mediator.Send(new Application.Features.CareerPaths.Commands.GenerateAiCatalog.GenerateAiCatalogCommand());
			return Ok(BaseResponse<Application.DTOs.Ai.CatalogGenerationResultDto>.SuccessResponse(result, result.Message));
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<CareerPathDTO>>> GetById(Guid id)
		{
			var result = await _mediator.Send(new GetCareerPathByIdQuery(id));
			return Ok(BaseResponse<CareerPathDTO>.SuccessResponse(result, "Career path retrieved successfully."));
		}

		[HttpPost]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("multipart/form-data")]
		public async Task<ActionResult<BaseResponse<Guid>>> CreateCareerPath([FromForm] CreateCareerPathCommand command)
		{
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("multipart/form-data")]
		public async Task<ActionResult<BaseResponse<string>>> UpdateCareerPath(Guid id, [FromForm] UpdateCareerPathCommand command)
		{
			if (id != command.Id)
				return BadRequest("Route ID and Command ID must match.");

			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpDelete("{id}")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		public async Task<ActionResult<BaseResponse<string>>> DeleteCareerPath(Guid id)
		{
			var command = new DeleteCareerPathCommand(id);
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		// ───────── Track Management ─────────

		[HttpGet("{careerPathId}/tracks")]
		public async Task<ActionResult<BaseResponse<List<CareerPathTrackDTO>>>> GetTracks(Guid careerPathId)
		{
			var result = await _mediator.Send(new GetTracksByCareerPathQuery(careerPathId));
			return Ok(BaseResponse<List<CareerPathTrackDTO>>.SuccessResponse(result, "Tracks retrieved successfully."));
		}

		[HttpPost("{careerPathId}/tracks")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("multipart/form-data")]
		public async Task<ActionResult<BaseResponse<Guid>>> CreateTrack(Guid careerPathId, [FromForm] CreateCareerPathTrackCommand command)
		{
			if (careerPathId != command.CareerPathId)
				return BadRequest("Route CareerPathId and Command CareerPathId must match.");

			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPost("{careerPathId}/tracks/{trackId}/skills")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<Guid>>> AddSkillToTrack(Guid careerPathId, Guid trackId, [FromBody] AddSkillToTrackCommand command)
		{
			if (careerPathId != command.CareerPathId || trackId != command.TrackId)
				return BadRequest("Route IDs and Command IDs must match.");

			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpDelete("{careerPathId}/tracks/{trackId}")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		public async Task<ActionResult<BaseResponse<string>>> DeleteTrack(Guid careerPathId, Guid trackId)
		{
			var track = await _unitOfWork.CareerPathTracks.GetByIdAsync(trackId);
			if (track == null || track.CareerPathId != careerPathId) return NotFound("Track not found.");
			
			await _unitOfWork.CareerPathTracks.DeleteAsync(track);
			await _unitOfWork.SaveChangesAsync(default);
			return Ok(BaseResponse<string>.SuccessResponse("Track deleted successfully.", "Track deleted successfully."));
		}

		[HttpDelete("{careerPathId}/tracks/{trackId}/skills/{skillId}")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		public async Task<ActionResult<BaseResponse<string>>> RemoveSkillFromTrack(Guid careerPathId, Guid trackId, Guid skillId)
		{
			var careerPathSkills = await _unitOfWork.CareerPathSkills.FindAsync(cps => 
				cps.CareerPathId == careerPathId && 
				cps.CareerPathTrackId == trackId && 
				cps.SkillId == skillId);
			
			var skillToRemove = careerPathSkills.FirstOrDefault();
			if (skillToRemove == null) return NotFound("Skill not found in this track.");
			
			await _unitOfWork.CareerPathSkills.DeleteAsync(skillToRemove);
			await _unitOfWork.SaveChangesAsync(default);
			return Ok(BaseResponse<string>.SuccessResponse("Skill removed from track.", "Skill removed from track."));
		}

		// ───────── Career Path Assignment ─────────

		[HttpPost("assign-learner")]
		[Authorize(Roles = "Learner,Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<Guid>>> AssignToLearner([FromBody] AssignCareerPathToLearnerCommand command)
		{
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPost("assign-team-member")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<Guid>>> AssignToTeamMember([FromBody] AssignCareerPathToTeamMemberCommand command)
		{
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPost("unassign-learner")]
		[Authorize(Roles = "Learner,Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<string>>> UnassignFromLearner([FromBody] UnassignCareerPathFromLearnerCommand command)
		{
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPost("unassign-team-member")]
		[Authorize(Roles = "Manager,Admin,SuperAdmin")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<string>>> UnassignFromTeamMember([FromBody] UnassignCareerPathFromTeamMemberCommand command)
		{
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		// ───────── Assigned Career Path Queries ─────────

		[HttpGet("assigned/learner/{learnerId}")]
		public async Task<ActionResult<BaseResponse<List<AssignedCareerPathDTO>>>> GetAssignedByLearner(Guid learnerId)
		{
			var result = await _mediator.Send(new GetAssignedCareerPathsByLearnerQuery(learnerId));
			return Ok(BaseResponse<List<AssignedCareerPathDTO>>.SuccessResponse(result, "Assigned career paths retrieved successfully."));
		}

		[HttpGet("assigned/team-member/{teamMemberId}")]
		public async Task<ActionResult<BaseResponse<List<AssignedCareerPathDTO>>>> GetAssignedByTeamMember(Guid teamMemberId)
		{
			var result = await _mediator.Send(new GetAssignedCareerPathsByTeamMemberQuery(teamMemberId));
			return Ok(BaseResponse<List<AssignedCareerPathDTO>>.SuccessResponse(result, "Assigned career paths retrieved successfully."));
		}
	}
}

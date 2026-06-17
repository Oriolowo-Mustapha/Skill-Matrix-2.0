using Application.DTOs;
using Application.Features.Assessments.Commands.StartAssessment;
using Application.Features.Assessments.Commands.SubmitAssessment;
using Application.Features.Assessments.Queries.GetAssessmentResult;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Skill_Matrix_2_0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class AssessmentsController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly Application.Interfaces.Service.ICodeExecutionService _codeExecutionService;

		public AssessmentsController(IMediator mediator, Application.Interfaces.Service.ICodeExecutionService codeExecutionService)
		{
			_mediator = mediator;
			_codeExecutionService = codeExecutionService;
		}

		[HttpPost("start")]
		[Consumes("application/json")]
		public async Task<IActionResult> StartAssessment([FromBody] AssesmentDTO assesmentDto)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var command = new StartAssessmentCommand(assesmentDto, userId, userRole);
			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpPost("submit")]
		[Consumes("application/json")]
		public async Task<IActionResult> SubmitAssessment([FromBody] SubmitAssessmentRequestDTO submitDto)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var command = new SubmitAssessmentCommand(submitDto, userId, userRole);
			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpPost("improvement-check/start")]
		public async Task<IActionResult> StartImprovementCheck([FromQuery] Guid skillId, [FromQuery] string concept)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var command = new Application.Features.Assessments.Commands.StartImprovementCheck.StartImprovementCheckCommand(skillId, concept, userId, userRole);
			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpPost("improvement-check/submit")]
		[Consumes("application/json")]
		public async Task<IActionResult> SubmitImprovementCheck([FromBody] SubmitAssessmentRequestDTO submitDto)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var command = new Application.Features.Assessments.Commands.SubmitImprovementCheck.SubmitImprovementCheckCommand(submitDto, userId, userRole);
			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpGet("results/{id}")]
		public async Task<IActionResult> GetAssessmentResult(Guid id)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);

			var query = new GetAssessmentResultQuery(id, userId);
			var response = await _mediator.Send(query);
			return Ok(response);
		}

		[HttpPost("run-code")]
		[Consumes("application/json")]
		public async Task<ActionResult<Application.DTOs.Assessments.CodeExecutionResponseDTO>> RunCode([FromBody] Application.DTOs.Assessments.CodeExecutionRequestDTO request)
		{
			var response = await _codeExecutionService.ExecuteCodeAsync(request);
			return Ok(response);
		}
	}
}
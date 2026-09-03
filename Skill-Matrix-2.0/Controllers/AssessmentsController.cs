using Application.DTOs;
using Application.DTOs.Assessments;
using Application.Features.Assessments.Commands.GenerateStarterPlan;
using Application.Features.Assessments.Commands.SaveQuestionResponse;
using Application.Features.Assessments.Commands.StartAssessment;
using Application.Features.Assessments.Commands.StartImprovementCheck;
using Application.Features.Assessments.Commands.StartTrackBaseline;
using Application.Features.Assessments.Commands.SubmitAssessment;
using Application.Features.Assessments.Commands.SubmitImprovementCheck;
using Application.Features.Assessments.Queries.GetAssessmentAttemptState;
using Application.Features.Assessments.Queries.GetAssessmentDetail;
using Application.Features.Assessments.Queries.GetAssessmentHistory;
using Application.Features.Assessments.Queries.GetAssessmentResult;
using Application.Interfaces.Service;
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
		private readonly ICodeExecutionService _codeExecutionService;

		public AssessmentsController(IMediator mediator, ICodeExecutionService codeExecutionService)
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

		[HttpPost("generate-starter-plan")]
		[Consumes("application/json")]
		public async Task<IActionResult> GenerateStarterPlan([FromBody] GenerateStarterPlanRequestDTO dto)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var command = new GenerateStarterPlanCommand(dto, userId, userRole);
			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpPost("track-baseline/start")]
		[Consumes("application/json")]
		public async Task<IActionResult> StartTrackBaseline([FromBody] StartTrackBaselineRequestDTO dto)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var command = new StartTrackBaselineCommand 
			{ 
				Dto = dto, 
				UserId = userId, 
				UserRole = userRole 
			};
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

			var command = new StartImprovementCheckCommand(skillId, concept, userId, userRole);
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

			var command = new SubmitImprovementCheckCommand(submitDto, userId, userRole);
			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpGet("history")]
		public async Task<IActionResult> GetAssessmentHistory()
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var query = new GetAssessmentHistoryQuery(userId, userRole);
			var response = await _mediator.Send(query);
			return Ok(response);
		}

		[HttpGet("results/{id}")]
		public async Task<ActionResult<BaseResponse<AssessmentResultDTO>>> GetAssessmentResult(Guid id)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);

			var query = new GetAssessmentResultQuery(id, userId);
			var response = await _mediator.Send(query);
			return Ok(BaseResponse<AssessmentResultDTO>.SuccessResponse(response, "Assessment result retrieved successfully."));
		}

		[HttpGet("results/{id}/details")]
		[HttpGet("history/{id}/details")]
		public async Task<IActionResult> GetAssessmentDetail(Guid id)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var query = new GetAssessmentDetailQuery(id, userId, userRole);
			var response = await _mediator.Send(query);
			return Ok(response);
		}

		[HttpGet("batches/{batchId}/state")]
		public async Task<IActionResult> GetAttemptState(int batchId)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var query = new GetAssessmentAttemptStateQuery(batchId, userId, userRole);
			var response = await _mediator.Send(query);
			return Ok(response);
		}

		[HttpPut("batches/{batchId}/responses/{questionId}")]
		[Consumes("application/json")]
		public async Task<IActionResult> SaveQuestionResponse(int batchId, int questionId, [FromBody] SaveQuestionResponseDTO dto)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var command = new SaveQuestionResponseCommand(batchId, questionId, dto, userId, userRole);
			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpPost("run-code")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<CodeExecutionResponseDTO>>> RunCode([FromBody] CodeExecutionRequestDTO request)
		{
			var response = await _codeExecutionService.ExecuteCodeAsync(request);
			return Ok(BaseResponse<CodeExecutionResponseDTO>.SuccessResponse(response, "Code executed successfully."));
		}
	}
}
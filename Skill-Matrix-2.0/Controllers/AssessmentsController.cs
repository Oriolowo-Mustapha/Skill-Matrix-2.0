using Application.DTOs;
using Application.Features.Assessments.Commands.StartAssessment;
using Application.Features.Assessments.Commands.StartTrackBaseline;
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

		[HttpPost("generate-starter-plan")]
		[Consumes("application/json")]
		public async Task<IActionResult> GenerateStarterPlan([FromBody] GenerateStarterPlanRequestDTO dto)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var command = new Application.Features.Assessments.Commands.GenerateStarterPlan.GenerateStarterPlanCommand(dto, userId, userRole);
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

		[HttpGet("history")]
		public async Task<IActionResult> GetAssessmentHistory()
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var query = new Application.Features.Assessments.Queries.GetAssessmentHistory.GetAssessmentHistoryQuery(userId, userRole);
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

			var query = new Application.Features.Assessments.Queries.GetAssessmentDetail.GetAssessmentDetailQuery(id, userId, userRole);
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

			var query = new Application.Features.Assessments.Queries.GetAssessmentAttemptState.GetAssessmentAttemptStateQuery(batchId, userId, userRole);
			var response = await _mediator.Send(query);
			return Ok(response);
		}

		[HttpPut("batches/{batchId}/responses/{questionId}")]
		[Consumes("application/json")]
		public async Task<IActionResult> SaveQuestionResponse(int batchId, int questionId, [FromBody] Application.DTOs.SaveQuestionResponseDTO dto)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new UnauthorizedAccessException("User ID claim not found.");
			var userId = Guid.Parse(userIdClaim);
			var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

			var command = new Application.Features.Assessments.Commands.SaveQuestionResponse.SaveQuestionResponseCommand(batchId, questionId, dto, userId, userRole);
			var response = await _mediator.Send(command);
			return Ok(response);
		}

		[HttpPost("run-code")]
		[Consumes("application/json")]
		public async Task<ActionResult<BaseResponse<Application.DTOs.Assessments.CodeExecutionResponseDTO>>> RunCode([FromBody] Application.DTOs.Assessments.CodeExecutionRequestDTO request)
		{
			var response = await _codeExecutionService.ExecuteCodeAsync(request);
			return Ok(BaseResponse<Application.DTOs.Assessments.CodeExecutionResponseDTO>.SuccessResponse(response, "Code executed successfully."));
		}
	}
}
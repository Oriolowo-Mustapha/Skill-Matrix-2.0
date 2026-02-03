using Application.DTOs;
using Application.Features.Assessments.Commands.StartAsssessment;
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

		public AssessmentsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("start")]
		public async Task<IActionResult> StartAssessment([FromBody] AssesmentDTO assesmentDto)
		{
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
			var userRole = User.FindFirstValue(ClaimTypes.Role);

			var command = new StartAssessmentCommand(assesmentDto, userId, userRole);
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPost("submit")]
		public async Task<IActionResult> SubmitAssessment([FromBody] SubmitAssessmentRequestDTO submitDto)
		{
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
			var userRole = User.FindFirstValue(ClaimTypes.Role);

			var command = new SubmitAssessmentCommand(submitDto, userId, userRole);
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpGet("results/{id}")]
		public async Task<IActionResult> GetAssessmentResult(Guid id)
		{
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

			var query = new GetAssessmentResultQuery(id, userId);
			var result = await _mediator.Send(query);
			return Ok(result);
		}
	}
}

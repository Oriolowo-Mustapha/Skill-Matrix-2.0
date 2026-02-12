using Application.DTOs;
using Application.Features.Auth.Commands.ForgotPassword;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.RegisterLearner;
using Application.Features.Auth.Commands.ResetPassword;
using Application.Features.Auth.Commands.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace Skill_Matrix_2._0.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IMediator _mediator;

		public AuthController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("register-learner")]
		public async Task<ActionResult<UserDTO>> RegisterLearner([FromBody] RegisterLearnerRequestDTO request)
		{
			var command = new RegisterLearnerCommand(request);
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPost("login")]
		public async Task<ActionResult<UserDTO>> Login([FromBody] LoginRequestDTO request)
		{
			var command = new LoginCommand(request);
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpGet("verify-email")]
		public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromQuery] string email)
		{
			var command = new VerifyEmailCommand(email, token);
			await _mediator.Send(command);
			return Ok(new { Message = "Email verified successfully. You can now log in." });
		}

		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
		{
			var command = new ForgotPasswordCommand(request.Email);
			await _mediator.Send(command);
			return Ok(new { Message = "If an account with that email exists, a password reset link has been sent." });
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
		{
			var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
			await _mediator.Send(command);
			return Ok(new { Message = "Password has been reset successfully. You can now log in with your new password." });
		}

		[HttpGet("google-login")]
		public IActionResult GoogleLogin()
		{
			var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleLoginCallback)) };
			return Challenge(properties, GoogleDefaults.AuthenticationScheme);
		}

		[HttpGet("google-login-callback")]
		public async Task<IActionResult> GoogleLoginCallback()
		{
			var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			if (!authenticateResult.Succeeded)
			{
				return BadRequest("External authentication failed.");
			}

			var email = authenticateResult.Principal.FindFirst(claim => claim.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
			var name = authenticateResult.Principal.FindFirst(claim => claim.Type == System.Security.Claims.ClaimTypes.Name)?.Value;

			return Ok(new { Message = $"Successfully authenticated with Google. Email: {email}, Name: {name}" });
		}
	}
}
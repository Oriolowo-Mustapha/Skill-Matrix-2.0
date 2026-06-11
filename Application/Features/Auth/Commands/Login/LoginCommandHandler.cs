using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Application.Features.Auth.Commands.Login
{
	public class LoginCommandHandler : IRequestHandler<LoginCommand, BaseResponse<LoginResponseDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IConfiguration _configuration;

		public LoginCommandHandler(IUnitOfWork unitOfWork, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_configuration = configuration;
		}

		public async Task<BaseResponse<LoginResponseDTO>> Handle(LoginCommand request, CancellationToken cancellationToken)
		{
			UserDTO? userDto = null;
			string? passwordHash = null;
			bool isEmailVerified = false;
			List<string> roles = new List<string>();

			if (!string.IsNullOrWhiteSpace(request.req.Email))
			{
				var learner = await _unitOfWork.Learners.GetByEmailAsync(request.req.Email);
				if (learner != null)
				{
					userDto = learner.ToDto();
					passwordHash = learner.PasswordHash;
					isEmailVerified = learner.IsEmailVerified;
					roles.Add(learner.Role);
				}
				else
				{
					var teamMember = await _unitOfWork.TeamMembers.GetByEmailAsync(request.req.Email);
					if (teamMember != null)
					{
						userDto = teamMember.ToDto();
						passwordHash = teamMember.PasswordHash;
						isEmailVerified = teamMember.IsEmailVerified;
						roles.Add(teamMember.Role);
					}
					else
					{
						var manager = await _unitOfWork.ManagerRepository.GetByEmailAsync(request.req.Email);
						if (manager != null)
						{
							userDto = manager.ToDto();
							passwordHash = manager.PasswordHash;
							isEmailVerified = manager.IsEmailVerified;
							roles.Add(manager.Role.ToString());
						}
						else
						{
							var admin = await _unitOfWork.Admins.GetByEmailAsync(request.req.Email);
							if (admin != null)
							{
								userDto = admin.ToDto();
								passwordHash = admin.PasswordHash;
								isEmailVerified = true; // Admins don't have this field, assume true
								roles.Add(admin.Role);
							}
						}
					}
				}
			}

			if (userDto == null && !string.IsNullOrWhiteSpace(request.req.UserName))
			{
				var learner = await _unitOfWork.Learners.GetByUserName(request.req.UserName);
				if (learner != null)
				{
					userDto = learner.ToDto();
					passwordHash = learner.PasswordHash;
					isEmailVerified = learner.IsEmailVerified;
					roles.Add(learner.Role);
				}
				else
				{
					var teamMember = await _unitOfWork.TeamMembers.GetByUserNameAsync(request.req.UserName);
					if (teamMember != null)
					{
						userDto = teamMember.ToDto();
						passwordHash = teamMember.PasswordHash;
						isEmailVerified = teamMember.IsEmailVerified;
						roles.Add(teamMember.Role);
					}
					else
					{
						var manager = await _unitOfWork.ManagerRepository.GetByUsernameAsync(request.req.UserName);
						if (manager != null)
						{
							userDto = manager.ToDto();
							passwordHash = manager.PasswordHash;
							isEmailVerified = manager.IsEmailVerified;
							roles.Add(manager.Role.ToString());
						}
						else
						{
							var admin = await _unitOfWork.Admins.GetByUserNameAsync(request.req.UserName);
							if (admin != null)
							{
								userDto = admin.ToDto();
								passwordHash = admin.PasswordHash;
								isEmailVerified = true;
								roles.Add(admin.Role);
							}
						}
					}
				}
			}

			if (userDto == null || !VerifyPassword(request.req.Password, passwordHash))
			{
				throw new UnauthorizedException("Invalid email or password.");
			}

			if (!isEmailVerified)
			{
				throw new ForbiddenException("Please verify your email address before logging in.");
			}

			var token = GenerateJwtToken(userDto.Id.ToString(), userDto.Email, roles, _configuration);

			return BaseResponse<LoginResponseDTO>.SuccessResponse(new LoginResponseDTO
			{
				Token = token,
				User = userDto
			}, "Login successful.");
		}


		private bool VerifyPassword(string password, string? hashedPassword)
		{
			if (string.IsNullOrEmpty(hashedPassword))
			{
				return false;
			}

			return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
		}

		private static string GenerateJwtToken(string userId, string email, IEnumerable<string> roles, IConfiguration configuration)
		{
			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, userId),
				new Claim(JwtRegisteredClaimNames.Email, email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured."))
			);

			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: configuration["Jwt:Issuer"],
				audience: configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(
					int.Parse(configuration["Jwt:ExpiryMinutes"] ?? "60")
				),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
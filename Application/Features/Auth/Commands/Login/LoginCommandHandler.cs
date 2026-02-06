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
	public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDTO>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IConfiguration _configuration;

		public LoginCommandHandler(IUnitOfWork unitOfWork, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_configuration = configuration;
		}

		public async Task<LoginResponseDTO> Handle(LoginCommand request, CancellationToken cancellationToken)
		{
			// A common user interface or base class would be better, but this works for now.
			UserDTO userDto = null;
			string passwordHash = null;
			List<string> roles = new List<string>();

			var learner = await _unitOfWork.Learners.GetByEmailAsync(request.req.Email);
			if (learner != null)
			{
				userDto = learner.ToDto();
				passwordHash = learner.PasswordHash;
				roles.Add(learner.Role);
			}
			else
			{
				var teamMember = await _unitOfWork.TeamMembers.GetByEmailAsync(request.req.Email);
				if (teamMember != null)
				{
					userDto = teamMember.ToDto();
					passwordHash = teamMember.PasswordHash;
					roles.Add(teamMember.Role);
				}
				else
				{
					var manager = await _unitOfWork.ManagerRepository.GetByEmailAsync(request.req.Email);
					if (manager != null)
					{
						userDto = manager.ToDto();
						passwordHash = manager.PasswordHash;
						roles.Add(manager.Role.ToString());
					}
				}
			}

			if (userDto == null || !VerifyPassword(request.req.Password, passwordHash))
			{
				throw new UnauthorizedException("Invalid email or password.");
			}

			var token = GenerateJwtToken(userDto.Id.ToString(), userDto.Email, roles, _configuration);

			return new LoginResponseDTO
			{
				Token = token,
				User = userDto
			};
		}


		private bool VerifyPassword(string password, string hashedPassword)
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
				Encoding.UTF8.GetBytes(configuration["Jwt:Key"])
			);

			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: configuration["Jwt:Issuer"],
				audience: configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(
					int.Parse(configuration["Jwt:ExpiryMinutes"])
				),
				signingCredentials: creds
			);

			return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}

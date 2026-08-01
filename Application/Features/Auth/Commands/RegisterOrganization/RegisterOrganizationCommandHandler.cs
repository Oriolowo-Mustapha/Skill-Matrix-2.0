using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Auth.Commands.RegisterOrganization
{
	public class RegisterOrganizationCommandHandler : IRequestHandler<RegisterOrganizationCommand, BaseResponse<string>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailService _emailService;
		private readonly IPhotoService _photoService;
		private readonly IConfiguration _configuration;

		public RegisterOrganizationCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService, IPhotoService photoService, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_emailService = emailService;
			_photoService = photoService;
			_configuration = configuration;
		}

		public async Task<BaseResponse<string>> Handle(RegisterOrganizationCommand command, CancellationToken cancellationToken)
		{
			var request = command.Request;

			var orgExists = await _unitOfWork.Organizations.ExistsAsync(o => o.Name == request.OrganizationName);
			if (orgExists)
			{
				throw new ConflictException($"Organization with name '{request.OrganizationName}' already exists.");
			}
			var email = request.ManagerEmail.Trim().ToLowerInvariant();
			var userName = request.ManagerUserName.Trim().ToLowerInvariant();

			bool emailExists = await _unitOfWork.Learners.GetByEmailAsync(email) != null ||
			                  await _unitOfWork.TeamMembers.GetByEmailAsync(email) != null ||
			                  await _unitOfWork.ManagerRepository.GetByEmailAsync(email) != null ||
			                  await _unitOfWork.Admins.GetByEmailAsync(email) != null;
			if (emailExists)
			{
				throw new ConflictException($"User with email '{request.ManagerEmail}' already exists.");
			}

			bool usernameExists = await _unitOfWork.Learners.GetByUserName(userName) != null ||
			                     await _unitOfWork.TeamMembers.GetByUserNameAsync(userName) != null ||
			                     await _unitOfWork.ManagerRepository.GetByUsernameAsync(userName) != null ||
			                     await _unitOfWork.Admins.GetByUserNameAsync(userName) != null;
			if (usernameExists)
			{
				throw new ConflictException($"User with username '{request.ManagerUserName}' already exists.");
			}
			string? orgProfilePicUrl = null;
			if (request.OrganizationProfilePicture != null)
			{
				orgProfilePicUrl = await _photoService.AddPhotoAsync(request.OrganizationProfilePicture);
			}

			var newOrganization = new Organization
			{
				Name = request.OrganizationName,
				Description = request.OrganizationDescription,
				ProfilePictureUrl = orgProfilePicUrl,
				DateJoined = DateTime.UtcNow
			};

			var hashedPassword = HashPassword(request.ManagerPassword);
			var verificationToken = Guid.NewGuid().ToString();

			var newManager = new Manager
			{
				FirstName = request.ManagerFirstName,
				LastName = request.ManagerLastName,
				Email = email,
				UserName = userName,
				PasswordHash = hashedPassword,
				Organization = newOrganization,
				IsEmailVerified = false,
				EmailVerificationToken = verificationToken,
				EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
			};

			await _unitOfWork.Organizations.AddAsync(newOrganization);
			await _unitOfWork.ManagerRepository.AddAsync(newManager);

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var frontendUrl = (_configuration["AppUrls:FrontendUrl"] ?? "http://localhost:5173").TrimEnd('/');
			var verificationLink = $"{frontendUrl}/verify-email?token={verificationToken}&email={Uri.EscapeDataString(newManager.Email)}";
			var subject = $"Welcome to Skill Matrix 2.0 - {newOrganization.Name}!";
			var body = $"""
				Dear {newManager.UserName},

				Welcome to Skill Matrix 2.0! Your organization, {newOrganization.Name}, has been successfully registered.

				To complete your registration, please verify your email by clicking the link below:
				{verificationLink}

				This link will expire in 24 hours.

				If you have any questions or need assistance, please do not hesitate to contact our support team.

				Best regards,
				The Skill Matrix 2.0 Team
				""";

			await _emailService.SendEmailAsync(newManager.Email, subject, body);

			return BaseResponse<string>.SuccessResponse(" ", "Registration Successful. You can now check your email to verify.");
		}

		private string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}
	}
}
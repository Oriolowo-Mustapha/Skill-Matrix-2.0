using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.RegisterTeamMember
{
	public class CreateTeamMemberCommandHandler : IRequestHandler<CreateTeamMemberCommand, BaseResponse<string>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailService _emailService;
		private readonly IPhotoService _photoService;

		public CreateTeamMemberCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService, IPhotoService photoService)
		{
			_unitOfWork = unitOfWork;
			_emailService = emailService;
			_photoService = photoService;
		}
		public async Task<BaseResponse<string>> Handle(CreateTeamMemberCommand request, CancellationToken cancellationToken)
		{
			var getManager = await _unitOfWork.ManagerRepository.GetByIdAsync(request.ManagerId);
			if (getManager == null)
			{
				throw new UnauthorizedException("Unauthorized: No manager found with the provided ID.");
			}
			var email = request.request.Email.Trim().ToLowerInvariant();
			var userName = request.request.UserName.Trim().ToLowerInvariant();

			bool emailExists = await _unitOfWork.Learners.GetByEmailAsync(email) != null ||
			                  await _unitOfWork.TeamMembers.GetByEmailAsync(email) != null ||
			                  await _unitOfWork.ManagerRepository.GetByEmailAsync(email) != null ||
			                  await _unitOfWork.Admins.GetByEmailAsync(email) != null;
			if (emailExists)
			{
				throw new ConflictException($"User with email '{request.request.Email}' already exists.");
			}

			bool usernameExists = await _unitOfWork.Learners.GetByUserName(userName) != null ||
			                     await _unitOfWork.TeamMembers.GetByUserNameAsync(userName) != null ||
			                     await _unitOfWork.ManagerRepository.GetByUsernameAsync(userName) != null ||
			                     await _unitOfWork.Admins.GetByUserNameAsync(userName) != null;
			if (usernameExists)
			{
				throw new ConflictException($"User with username '{request.request.UserName}' already exists.");
			}

			var getOrganization = await _unitOfWork.Organizations.GetByIdAsync(getManager.OrganizationId);
			if (getOrganization == null)
			{
				throw new System.ApplicationException("Could not find the organization associated with the manager.");
			}

			var hashedPassword = HashPassword(request.request.Password);
			var verificationToken = Guid.NewGuid().ToString();

            string? profilePicUrl = null;
            if (request.request.ProfilePicUrl != null)
            {
                profilePicUrl = await _photoService.AddPhotoAsync(request.request.ProfilePicUrl);
            }

            var newTeamMember = new TeamMember
			{
				FirstName = request.request.FirstName,
				LastName = request.request.LastName,
				Email = email,
				UserName = userName,
				ProfilePictureUrl = profilePicUrl,
				PasswordHash = hashedPassword,
				Manager = getManager,
				Organization = getOrganization,
				IsEmailVerified = false,
				EmailVerificationToken = verificationToken,
				EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
			};

			await _unitOfWork.TeamMembers.AddAsync(newTeamMember);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var toDTO = new TeamMemberDTO
			{
				Id = newTeamMember.Id,
				FirstName = newTeamMember.FirstName,
				LastName = newTeamMember.LastName,
				Email = newTeamMember.Email,
				UserName = newTeamMember.UserName,
				ProfilePicUrl = newTeamMember.ProfilePictureUrl,
				OrganizationId = newTeamMember.OrganizationId,
				ManagerId = newTeamMember.ManagerId
			};

			var verificationLink = $"https://yourdomain.com/api/auth/verify-email?token={verificationToken}&email={newTeamMember.Email}";
			var subject = $"You're Invited to Skill Matrix 2.0 - Join {getOrganization.Name}!";
			var body = $"""
				Dear {newTeamMember.UserName},

				You have been invited by {getManager.UserName} to join {getOrganization.Name} on Skill Matrix 2.0.

				Here are your account credentials:
				- Username: {newTeamMember.UserName}
				- Email: {newTeamMember.Email}
				- Password: {request.request.Password}

				To activate your account, please verify your email by clicking the link below:
				{verificationLink}

				This link will expire in 24 hours.

				If you have any questions, please contact your manager, {getManager.UserName}, or our support team.

				Best regards,
				The Skill Matrix 2.0 Team
				""";

			await _emailService.SendEmailAsync(newTeamMember.Email, subject, body);
			return BaseResponse<string>.SuccessResponse(" ", "Team member successfully registered.");
		}

		private string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}
	}
}
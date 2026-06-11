using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.RegisterLearner
{
	public class RegisterLearnerCommandHandler : IRequestHandler<RegisterLearnerCommand, BaseResponse<string>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailService _emailService;
		private readonly IPhotoService _photoService;

		public RegisterLearnerCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService, IPhotoService photoService)
		{
			_unitOfWork = unitOfWork;
			_emailService = emailService;
			_photoService = photoService;
		}

		public async Task<BaseResponse<string>> Handle(RegisterLearnerCommand request, CancellationToken cancellationToken)
		{
			var learnerExits = await _unitOfWork.Learners.GetByEmailAsync(request.req.Email);
			if (learnerExits != null)
			{
				throw new ConflictException($"User with {request.req.Email} already exists.");
			}

			var hashedPassword = HashPassword(request.req.PasswordHash);
			var verificationToken = Guid.NewGuid().ToString();
			
			string? profilePicUrl = null;
			if (request.req.ProfilePic != null)
			{
				profilePicUrl = await _photoService.AddPhotoAsync(request.req.ProfilePic);
			}

			var learner = new Learner
			{
				FirstName = request.req.FirstName,
				LastName = request.req.LastName,
				Email = request.req.Email,
				UserName = request.req.UserName,
				Role = Domain.Enum.Roles.Learner.ToString(),
				ProfilePictureUrl = profilePicUrl,
				PasswordHash = hashedPassword,
				IsEmailVerified = false,
				EmailVerificationToken = verificationToken,
				EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
			};

			await _unitOfWork.Learners.AddAsync(learner);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var verificationLink = $"https://yourdomain.com/api/auth/verify-email?token={verificationToken}&email={learner.Email}";
			var subject = "Verify Your Email - Skill Matrix 2.0";
			var body = $"""
				Dear {learner.FirstName},

				Welcome to Skill Matrix 2.0! We are excited to have you on board.

				To complete your registration, please verify your email address by clicking the link below:
				{verificationLink}

				This link will expire in 24 hours.

				If you did not create an account, please ignore this email.

				Best regards,
				The Skill Matrix 2.0 Team
				""";
			await _emailService.SendEmailAsync(learner.Email, subject, body);

			return BaseResponse<string>.SuccessResponse(null, "Registration Successful. You can now check your email to verify.");
		}

		private string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}
	}
}
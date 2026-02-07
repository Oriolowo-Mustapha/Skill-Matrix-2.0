using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service; // Added
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.RegisterLearner
{
	public class RegisterLearnerCommandHandler : IRequestHandler<RegisterLearnerCommand, UserDTO>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailService _emailService;

		public RegisterLearnerCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_emailService = emailService;
		}

		public async Task<UserDTO> Handle(RegisterLearnerCommand request, CancellationToken cancellationToken)
		{
			var learnerExits = await _unitOfWork.Learners.GetByEmailAsync(request.req.Email);
			if (learnerExits != null)
			{
				throw new ConflictException($"User with {request.req.Email} already exists.");
			}

			var hashedPassword = HashPassword(request.req.PasswordHash);

			var learner = new Learner
			{
				FirstName = request.req.FirstName,
				LastName = request.req.LastName,
				Email = request.req.Email,
				UserName = request.req.UserName,
				Role = request.req.Role,
				ProfilePictureUrl = request.req.ProfilePicUrl,
				PasswordHash = hashedPassword
			};

			await _unitOfWork.Learners.AddAsync(learner);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var subject = "Welcome to Skill Matrix!";
			var body = $"""
				Dear {learner.FirstName},

				Welcome to Skill Matrix 2.0! We are excited to have you on board.

				Skill Matrix 2.0 is a platform designed to help you track and develop your professional skills.

				To get started, please log in with your registered email and explore the available skills and assessments.

				If you have any questions, please don't hesitate to reach out to our support team.

				We look forward to seeing your progress!

				Best regards,
				The Skill Matrix 2.0 Team
				""";
			await _emailService.SendEmailAsync(learner.Email, subject, body);

			return learner.ToDto();
		}

		private string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}
	}

}

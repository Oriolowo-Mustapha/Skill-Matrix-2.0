using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using MediatR;

namespace Application.Features.Auth.Commands.ForgotPassword
{
	public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailService _emailService;

		public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_emailService = emailService;
		}

		public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
		{
			var resetToken = Guid.NewGuid().ToString();
			var tokenExpiry = DateTime.UtcNow.AddHours(1);
			string? userEmail = null;
			string? userName = null;

			var learner = await _unitOfWork.Learners.GetByEmailAsync(request.Email);
			if (learner != null)
			{
				learner.PasswordResetToken = resetToken;
				learner.PasswordResetTokenExpiry = tokenExpiry;
				await _unitOfWork.Learners.UpdateAsync(learner);
				userEmail = learner.Email;
				userName = learner.UserName;
			}
			else
			{
				var teamMember = await _unitOfWork.TeamMembers.GetByEmailAsync(request.Email);
				if (teamMember != null)
				{
					teamMember.PasswordResetToken = resetToken;
					teamMember.PasswordResetTokenExpiry = tokenExpiry;
					await _unitOfWork.TeamMembers.UpdateAsync(teamMember);
					userEmail = teamMember.Email;
					userName = teamMember.UserName;
				}
				else
				{
					var manager = await _unitOfWork.ManagerRepository.GetByEmailAsync(request.Email);
					if (manager != null)
					{
						manager.PasswordResetToken = resetToken;
						manager.PasswordResetTokenExpiry = tokenExpiry;
						await _unitOfWork.ManagerRepository.UpdateAsync(manager);
						userEmail = manager.Email;
						userName = manager.UserName;
					}
				}
			}

			// Always return true to prevent email enumeration attacks
			if (userEmail == null)
				return true;

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var resetLink = $"https://yourdomain.com/reset-password?token={resetToken}&email={userEmail}";
			var subject = "Reset Your Password - Skill Matrix 2.0";
			var body = $"""
				Dear {userName},

				We received a request to reset your password for your Skill Matrix 2.0 account.

				To reset your password, please click the link below:
				{resetLink}

				This link will expire in 1 hour.

				If you did not request a password reset, please ignore this email. Your password will remain unchanged.

				Best regards,
				The Skill Matrix 2.0 Team
				""";

			await _emailService.SendEmailAsync(userEmail, subject, body);

			return true;
		}
	}
}
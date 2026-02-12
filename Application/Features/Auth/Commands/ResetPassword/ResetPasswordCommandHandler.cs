using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Auth.Commands.ResetPassword
{
	public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
	{
		private readonly IUnitOfWork _unitOfWork;

		public ResetPasswordCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
		{
			var learner = await _unitOfWork.Learners.GetByEmailAsync(request.Email);
			if (learner != null)
			{
				ValidateResetToken(learner.PasswordResetToken, learner.PasswordResetTokenExpiry, request.Token);

				learner.PasswordHash = HashPassword(request.NewPassword);
				learner.PasswordResetToken = null;
				learner.PasswordResetTokenExpiry = null;

				await _unitOfWork.Learners.UpdateAsync(learner);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				return true;
			}

			var teamMember = await _unitOfWork.TeamMembers.GetByEmailAsync(request.Email);
			if (teamMember != null)
			{
				ValidateResetToken(teamMember.PasswordResetToken, teamMember.PasswordResetTokenExpiry, request.Token);

				teamMember.PasswordHash = HashPassword(request.NewPassword);
				teamMember.PasswordResetToken = null;
				teamMember.PasswordResetTokenExpiry = null;

				await _unitOfWork.TeamMembers.UpdateAsync(teamMember);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				return true;
			}

			var manager = await _unitOfWork.ManagerRepository.GetByEmailAsync(request.Email);
			if (manager != null)
			{
				ValidateResetToken(manager.PasswordResetToken, manager.PasswordResetTokenExpiry, request.Token);

				manager.PasswordHash = HashPassword(request.NewPassword);
				manager.PasswordResetToken = null;
				manager.PasswordResetTokenExpiry = null;

				await _unitOfWork.ManagerRepository.UpdateAsync(manager);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				return true;
			}

			throw new NotFoundException($"No user found with email {request.Email}.");
		}

		private static void ValidateResetToken(string? storedToken, DateTime? tokenExpiry, string providedToken)
		{
			if (string.IsNullOrEmpty(storedToken))
				throw new BadRequestException("No password reset was requested for this account.");

			if (storedToken != providedToken)
				throw new BadRequestException("Invalid password reset token.");

			if (tokenExpiry < DateTime.UtcNow)
				throw new BadRequestException("Password reset token has expired.");
		}

		private static string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}
	}
}
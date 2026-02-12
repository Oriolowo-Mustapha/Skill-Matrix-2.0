using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Auth.Commands.VerifyEmail
{
	public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
	{
		private readonly IUnitOfWork _unitOfWork;

		public VerifyEmailCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
		{
			var learner = await _unitOfWork.Learners.GetByEmailAsync(request.Email);
			if (learner != null)
			{
				if (learner.IsEmailVerified)
					throw new ConflictException("Email is already verified.");

				if (learner.EmailVerificationToken != request.Token)
					throw new BadRequestException("Invalid verification token.");

				if (learner.EmailVerificationTokenExpiry < DateTime.UtcNow)
					throw new BadRequestException("Verification token has expired.");

				learner.IsEmailVerified = true;
				learner.EmailVerificationToken = null;
				learner.EmailVerificationTokenExpiry = null;

				await _unitOfWork.Learners.UpdateAsync(learner);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				return true;
			}

			var teamMember = await _unitOfWork.TeamMembers.GetByEmailAsync(request.Email);
			if (teamMember != null)
			{
				if (teamMember.IsEmailVerified)
					throw new ConflictException("Email is already verified.");

				if (teamMember.EmailVerificationToken != request.Token)
					throw new BadRequestException("Invalid verification token.");

				if (teamMember.EmailVerificationTokenExpiry < DateTime.UtcNow)
					throw new BadRequestException("Verification token has expired.");

				teamMember.IsEmailVerified = true;
				teamMember.EmailVerificationToken = null;
				teamMember.EmailVerificationTokenExpiry = null;

				await _unitOfWork.TeamMembers.UpdateAsync(teamMember);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				return true;
			}

			var manager = await _unitOfWork.ManagerRepository.GetByEmailAsync(request.Email);
			if (manager != null)
			{
				if (manager.IsEmailVerified)
					throw new ConflictException("Email is already verified.");

				if (manager.EmailVerificationToken != request.Token)
					throw new BadRequestException("Invalid verification token.");

				if (manager.EmailVerificationTokenExpiry < DateTime.UtcNow)
					throw new BadRequestException("Verification token has expired.");

				manager.IsEmailVerified = true;
				manager.EmailVerificationToken = null;
				manager.EmailVerificationTokenExpiry = null;

				await _unitOfWork.ManagerRepository.UpdateAsync(manager);
				await _unitOfWork.SaveChangesAsync(cancellationToken);
				return true;
			}

			throw new NotFoundException($"No user found with email {request.Email}.");
		}
	}
}
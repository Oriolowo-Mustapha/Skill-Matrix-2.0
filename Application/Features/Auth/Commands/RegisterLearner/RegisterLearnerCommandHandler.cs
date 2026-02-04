using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.RegisterLearner
{
	public class RegisterLearnerCommandHandler : IRequestHandler<RegisterLearnerCommand, UserDTO>
	{
		private readonly IUnitOfWork _unitOfWork;

		public RegisterLearnerCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
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
			return learner.ToDto();
		}

		private string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}
	}

}

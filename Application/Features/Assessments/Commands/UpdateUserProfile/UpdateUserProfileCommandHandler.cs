using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Assessments.Commands.UpdateUserProfile
{
	public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserDTO>
	{
		private readonly IUnitOfWork _unitOfWork;

		public UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<UserDTO> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
		{
			var learner = await _unitOfWork.Learners.GetByIdAsync(request.userId);

			if (learner != null)
			{
				learner.FirstName = request.Dto.FirstName;
				learner.LastName = request.Dto.LastName;
				learner.ProfilePictureUrl = request.Dto.ProfilePictureUrl;

				await _unitOfWork.Learners.UpdateAsync(learner);
				await _unitOfWork.SaveChangesAsync(cancellationToken);

				return learner.ToDto();
			}

			var teamMember = await _unitOfWork.TeamMembers.GetByIdAsync(request.userId);

			if (teamMember != null)
			{
				teamMember.FirstName = request.Dto.FirstName;
				teamMember.LastName = request.Dto.LastName;
				teamMember.ProfilePictureUrl = request.Dto.ProfilePictureUrl;

				await _unitOfWork.TeamMembers.UpdateAsync(teamMember);
				await _unitOfWork.SaveChangesAsync(cancellationToken);

				return teamMember.ToDto();
			}

			throw new NotFoundException("User", request.userId);
		}
	}
}

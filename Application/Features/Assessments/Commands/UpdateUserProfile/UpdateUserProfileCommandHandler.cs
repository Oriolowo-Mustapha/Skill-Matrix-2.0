using Application.DTOs;
using Application.Exceptions;
using Application.Extensions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using MediatR;

namespace Application.Features.Assessments.Commands.UpdateUserProfile
{
	public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserDTO>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPhotoService _photoService;

		public UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork, IPhotoService photoService)
		{
			_unitOfWork = unitOfWork;
			_photoService = photoService;
		}

		public async Task<UserDTO> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
		{
			string? newProfilePicUrl = null;
			if (request.Dto.ProfilePic != null)
			{
				newProfilePicUrl = await _photoService.AddPhotoAsync(request.Dto.ProfilePic);
			}

			var learner = await _unitOfWork.Learners.GetByIdAsync(request.userId);

			if (learner != null)
			{
				learner.FirstName = request.Dto.FirstName;
				learner.LastName = request.Dto.LastName;
				if (newProfilePicUrl != null)
				{
					learner.ProfilePictureUrl = newProfilePicUrl;
				}

				await _unitOfWork.Learners.UpdateAsync(learner);
				await _unitOfWork.SaveChangesAsync(cancellationToken);

				return learner.ToDto();
			}

			var teamMember = await _unitOfWork.TeamMembers.GetByIdAsync(request.userId);

			if (teamMember != null)
			{
				teamMember.FirstName = request.Dto.FirstName;
				teamMember.LastName = request.Dto.LastName;
				if (newProfilePicUrl != null)
				{
					teamMember.ProfilePictureUrl = newProfilePicUrl;
				}

				await _unitOfWork.TeamMembers.UpdateAsync(teamMember);
				await _unitOfWork.SaveChangesAsync(cancellationToken);

				return teamMember.ToDto();
			}

			var manager = await _unitOfWork.ManagerRepository.GetByIdAsync(request.userId);

			if (manager != null)
			{
				manager.FirstName = request.Dto.FirstName;
				manager.LastName = request.Dto.LastName;
				if (newProfilePicUrl != null)
				{
					manager.ProfilePictureUrl = newProfilePicUrl;
				}

				await _unitOfWork.ManagerRepository.UpdateAsync(manager);
				await _unitOfWork.SaveChangesAsync(cancellationToken);

				return manager.ToDto();
			}

			throw new NotFoundException("User", request.userId);
		}
	}
}

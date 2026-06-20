using Application.DTOs;
﻿using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Assessments.Commands.DeleteSkill
{
	public class DeleteSkillCommandHandler : IRequestHandler<DeleteSkillCommand, BaseResponse<string>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteSkillCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public async Task<BaseResponse<string>> Handle(DeleteSkillCommand request, CancellationToken cancellationToken)
		{
			var fetchSkill = await _unitOfWork.Skills.GetByIdAsync(request.Id);
			if (fetchSkill == null)
			{
				throw new NotFoundException("Skill", request.Id);
			}

			bool isAssigned = await _unitOfWork.AssignedSkills.ExistsAsync(x => x.SkillId == request.Id);

			if (isAssigned)
			{
				throw new ConflictException("Cannot delete this skill because it is currently assigned to users.");
			}
			await _unitOfWork.Skills.DeleteAsync(fetchSkill);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return BaseResponse<string>.SuccessResponse(" ", "Skill deleted successfully.");
		}
	}
}

using Application.DTOs;
﻿using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Assessments.Commands.UpdateSkill
{
	public class UpdateSkillCommandHandler : IRequestHandler<UpdateSkillCommand, BaseResponse<string>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public UpdateSkillCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public async Task<BaseResponse<string>> Handle(UpdateSkillCommand request, CancellationToken cancellationToken)
		{
			var fetchSkill = await _unitOfWork.Skills.GetByIdAsync(request.Id);
			if (fetchSkill == null)
			{
				throw new NotFoundException("Skill", request.Id);
			}

			var existSkill = await _unitOfWork.Skills.GetByNameAsync(request.Name);
			if (existSkill != null && existSkill.Id != request.Id)
			{
				throw new ConflictException("A skill with this name already exists");
			}

			fetchSkill.Name = request.Name;
			fetchSkill.Category = request.Category;

			await _unitOfWork.Skills.UpdateAsync(fetchSkill);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return BaseResponse<string>.SuccessResponse(" ", "Skill updated successfully.");
		}
	}
}

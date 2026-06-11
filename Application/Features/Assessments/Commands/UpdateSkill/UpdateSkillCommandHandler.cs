using Application.DTOs;
﻿using Application.Exceptions;
using Application.Interfaces.Repository;
using MediatR;

namespace Application.Features.Assessments.Commands.UpdateSkill
{
	public class UpdateSkillCommandHandler : IRequestHandler<UpdateSkillCommand, bool>
	{
		private readonly IUnitOfWork _unitOfWork;

		public UpdateSkillCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public async Task<bool> Handle(UpdateSkillCommand request, CancellationToken cancellationToken)
		{
			var fetchSkill = _unitOfWork.Skills.GetByIdAsync(request.Id);
			if (fetchSkill == null)
			{
				throw new NotFoundException("Skill", request.Id);
			}

			var existSkill = await _unitOfWork.Skills.GetByNameAsync(request.Name);
			if (existSkill.Id != request.Id)
			{
				throw new ConflictException("A skill with this name already exists");
			}

			existSkill.Name = request.Name;
			existSkill.Category = request.Category;

			await _unitOfWork.Skills.UpdateAsync(existSkill);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			return true;
		}
	}
}

using Application.DTOs;
﻿using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;

namespace Application.Features.Assessments.Commands.CreateSkill
{
	public class CreateSkillCommandHandler : IRequestHandler<CreateSkillCommand, BaseResponse<string>>
	{
		private readonly IUnitOfWork _unitOfWork;

		public CreateSkillCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<string>> Handle(CreateSkillCommand request, CancellationToken cancellationToken)
		{
			var existingSkill = await _unitOfWork.Skills.GetByNameAsync(request.Name);

			if (existingSkill != null)
			{
				throw new ConflictException($"A skill with the name '{request.Name.Trim()}' already exists.");
			}

			var newSkill = new Skill
			{
				Name = request.Name.Trim(),
				Category = request.Category,
				Source = "Admin",
				IsCustomized = true,
				DateAdded = DateTime.UtcNow
			};

			await _unitOfWork.Skills.AddAsync(newSkill);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return BaseResponse<string>.SuccessResponse(newSkill.Name, $"{newSkill.Name} has been created successfully.");
		}
	}
}

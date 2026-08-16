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
		private readonly IMediator _mediator;

		public CreateSkillCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
		{
			_unitOfWork = unitOfWork;
			_mediator = mediator;
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

			// Publish notification so Career Paths can sync incrementally
			await _mediator.Publish(new Application.Features.Skills.Notifications.SkillsAddedNotification(newSkill.Name, "Admin"), cancellationToken);

			return BaseResponse<string>.SuccessResponse(newSkill.Name, $"{newSkill.Name} has been created successfully.");
		}
	}
}

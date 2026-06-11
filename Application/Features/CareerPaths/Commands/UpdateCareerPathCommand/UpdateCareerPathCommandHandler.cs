using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;

namespace Application.Features.CareerPaths.Commands.UpdateCareerPathCommand
{
	public class UpdateCareerPathCommandHandler : IRequestHandler<UpdateCareerPathCommand>
	{
		private readonly IUnitOfWork _unitOfWork;

		public UpdateCareerPathCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task Handle(UpdateCareerPathCommand request, CancellationToken cancellationToken)
		{
			var careerPath = await _unitOfWork.CareerPaths.GetByIdAsync(request.Id);

			if (careerPath == null)
			{
				throw new NotFoundException($"CareerPath with Id {request.Id} not found.");
			}

			careerPath.Title = request.Title;
			careerPath.Description = request.Description;
			careerPath.IconURL = request.IconURL;

			// Manage Skill associations
			var existingCareerPathSkills = careerPath.CareerPathSkills.ToList();
			var existingSkillIds = existingCareerPathSkills.Select(cps => cps.SkillId).ToList();
			var skillsToAdd = request.SkillIds.Except(existingSkillIds).ToList();
			var skillsToRemove = existingSkillIds.Except(request.SkillIds).ToList();

			// Remove skills
			var careerPathSkillsToRemove = existingCareerPathSkills
				.Where(cps => skillsToRemove.Contains(cps.SkillId))
				.ToList();

			foreach (var careerPathSkill in careerPathSkillsToRemove)
			{
				careerPath.CareerPathSkills.Remove(careerPathSkill); // Remove from navigation property for EF to track deletion
			}
			// Add new skills
			foreach (var skillId in skillsToAdd)
			{
				var skillExists = await _unitOfWork.Skills.ExistsAsync(s => s.Id == skillId);
				if (!skillExists)
				{
					throw new BadRequestException($"Skill with ID {skillId} does not exist.");
				}
				careerPath.CareerPathSkills.Add(new CareerPathSkill { CareerPathId = careerPath.Id, SkillId = skillId });
			}

			await _unitOfWork.CareerPaths.UpdateAsync(careerPath);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
		}
	}
}

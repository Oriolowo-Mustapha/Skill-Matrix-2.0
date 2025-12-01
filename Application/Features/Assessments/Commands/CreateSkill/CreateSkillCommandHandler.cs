using Application.Exceptions;
using Application.Interfaces.Repository;
using Domain.Entities;
using MediatR;

namespace Application.Features.Assessments.Commands.CreateSkill
{
	public class CreateSkillCommandHandler : IRequestHandler<CreateSkillCommand, Guid>
	{
		private readonly IUnitOfWork _unitOfWork;

		public CreateSkillCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<Guid> Handle(CreateSkillCommand request, CancellationToken cancellationToken)
		{
			var existingSkill = await _unitOfWork.Skills.GetByNameAsync(request.Name);

			if (existingSkill != null)
			{
				throw new ConflictException($"A skill with the name '{request.Name}' already exists.");
			}

			var newSkill = new Skill
			{
				Name = request.Name,
				Category = request.Category,
				DateAdded = DateTime.UtcNow
			};

			await _unitOfWork.Skills.AddAsync(newSkill);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return newSkill.Id;
		}
	}
}

using Application.DTOs;
using Application.Extensions;
using Application.Features.Assessments.Commands.StartAsssessment;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;
using MediatR;

namespace Application.Features.Assessments.Commands.StartAssessment
{
	public class StartAssessmentCommandHandler : IRequestHandler<StartAssessmentCommand, StartAssesmentResponseDTO>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IAiService _aiService;

		public StartAssessmentCommandHandler(IUnitOfWork unitOfWork, IAiService aiService)
		{
			_unitOfWork = unitOfWork;
			_aiService = aiService;
		}

		public async Task<StartAssesmentResponseDTO> Handle(StartAssessmentCommand request, CancellationToken cancellationToken)
		{
			var assignedSkill = await _unitOfWork.AssignedSkills.GetByIdAsync(request.AssesmentDTO.AssignedSkillId);

			if (assignedSkill == null)
			{
				throw new KeyNotFoundException($"Assigned Skill with ID {request.AssesmentDTO.AssignedSkillId} not found.");
			}

			var questions = await _aiService.GeneratAssessmentQuestionsAsync(assignedSkill.Name, 10, assignedSkill.ProficiencyLevel.ToString());

			var batch = new AssessmentBatch
			{
				LearnerID = request.UserId,
				SkillId = assignedSkill.SkillId,
				AssessmentStatus = AssessmentStatus.InProgress,
				DateCreated = DateTime.UtcNow,
				Assessments = questions.ToList()
			};

			//await _unitOfWork.AssessmentBatches.AddAsync(batch);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			// 5. Map to Response DTO
			return batch.ToDTO(); // Using the Extension method we defined earlier!
		}
	}
}
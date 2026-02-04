using Application.DTOs;
using Application.Exceptions;
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

			var assignedSkill = await _unitOfWork.AssignedSkills.GetByUserAndSkillId(request.UserId, request.Dto.AssignedSkillId);

			if (assignedSkill == null)
			{
				throw new NotFoundException("Assigned Skill", request.Dto.AssignedSkillId);
			}

			var questions = await _aiService.GenerateAssessmentQuestionsAsync(
				assignedSkill.Name,
				assignedSkill.ProficiencyLevel.ToString(),
				10
			);

			var batch = new AssessmentBatch
			{
				SkillId = assignedSkill.SkillId,
				AssessmentStatus = AssessmentStatus.InProgress,
				DateCreated = DateTime.UtcNow,
				Assessments = questions.ToList()
			};

			if (request.UserRole == Roles.Learner.ToString())
			{
				batch.LearnerID = request.UserId;
			}
			else if (request.UserRole == Roles.Team_Members.ToString() || request.UserRole == "TeamMember")
			{
				batch.TeamMemberID = request.UserId;
			}
			else
			{
				throw new BadRequestException("Only Learners and Team Members can take assessments.");
			}


			await _unitOfWork.AssessmentBatches.AddAsync(batch);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			return batch.ToDTO();
		}
	}
}
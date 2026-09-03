using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Enum;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ImprovementPlans.Commands.CompleteImprovementTask
{
	public class CompleteImprovementTaskCommandHandler : IRequestHandler<CompleteImprovementTaskCommand, BaseResponse<string>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActivityLogService _activityLogService;

		public CompleteImprovementTaskCommandHandler(IUnitOfWork unitOfWork, IActivityLogService activityLogService)
		{
			_unitOfWork = unitOfWork;
			_activityLogService = activityLogService;
		}

		public async Task<BaseResponse<string>> Handle(CompleteImprovementTaskCommand request, CancellationToken cancellationToken)
		{
			var tasks = await _unitOfWork.ImprovementTasks.FindAsync(
				t => t.Id == request.TaskId,
				t => t.ImprovementPlan,
				t => t.ImprovementPlan.AssessmentResult
			);
			var task = tasks.FirstOrDefault();

			if (task == null)
			{
				throw new NotFoundException("Improvement Task", request.TaskId);
			}

			var result = task.ImprovementPlan.AssessmentResult;
			bool isOwner = request.UserRole == Roles.Learner.ToString()
				? result.LearnerID == request.UserId
				: result.TeamMemberID == request.UserId;

			if (!isOwner)
			{
				throw new UnauthorizedAccessException("You do not have permission to modify this task.");
			}

			if (task.Status == "Completed")
			{
				return BaseResponse<string>.SuccessResponse(" ", "Task is already completed.");
			}

			task.Status = "Completed";
			task.CompletedAt = DateTime.UtcNow;

			await _unitOfWork.ImprovementTasks.UpdateAsync(task);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			await _activityLogService.AwardPointsAsync(
				request.UserId,
				request.UserRole,
				Domain.Enum.UserActivityType.ImprovementTaskCompleted,
				$"Completed improvement task '{task.Concept}'.",
				20,
				"ImprovementTask",
				task.Id,
				cancellationToken);

			return BaseResponse<string>.SuccessResponse(" ", "Task marked as completed successfully.");
		}
	}
}

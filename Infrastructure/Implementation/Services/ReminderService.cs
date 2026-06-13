using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Enum;

namespace Infrastructure.Implementation.Services
{
	public class ReminderService : IReminderService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailService _emailService;

		public ReminderService(IUnitOfWork unitOfWork, IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_emailService = emailService;
		}

		public async Task SendWeeklyRemindersAsync()
		{
			// Get all pending assessments
			var allBatches = await _unitOfWork.AssessmentBatches.GetAllAsync();
			var pendingBatches = allBatches.Where(b => b.AssessmentStatus == AssessmentStatus.NotStarted || b.AssessmentStatus == AssessmentStatus.InProgress).ToList();

			// Group by User
			var learnerBatches = pendingBatches.Where(b => b.LearnerID.HasValue).GroupBy(b => b.LearnerID.Value);
			foreach (var group in learnerBatches)
			{
				var learner = await _unitOfWork.Learners.GetByIdAsync(group.Key);
				if (learner != null && !string.IsNullOrEmpty(learner.Email))
				{
					string body = $"Hello {learner.FirstName},<br/><br/>" +
						$"You have {group.Count()} pending skill assessments that need your attention. " +
						$"Please log in to the Skill Matrix platform to complete them and update your proficiency levels.<br/><br/>" +
						$"Best regards,<br/>The Skill Matrix Team";

					await _emailService.SendEmailAsync(learner.Email, "Action Required: Pending Skill Assessments", body);
				}
			}

			var tmBatches = pendingBatches.Where(b => b.TeamMemberID.HasValue).GroupBy(b => b.TeamMemberID.Value);
			foreach (var group in tmBatches)
			{
				var tm = await _unitOfWork.TeamMembers.GetByIdAsync(group.Key);
				if (tm != null && !string.IsNullOrEmpty(tm.Email))
				{
					string body = $"Hello {tm.FirstName},<br/><br/>" +
						$"You have {group.Count()} pending skill assessments that need your attention. " +
						$"Please log in to the Skill Matrix platform to complete them and update your proficiency levels.<br/><br/>" +
						$"Best regards,<br/>The Skill Matrix Team";

					await _emailService.SendEmailAsync(tm.Email, "Action Required: Pending Skill Assessments", body);
				}
			}
		}
	}
}

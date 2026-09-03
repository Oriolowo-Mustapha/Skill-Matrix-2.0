using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using Domain.Enum;
using MediatR;

namespace Application.Features.Gamification.Commands.EndorsePeer
{
	public class EndorsePeerCommandHandler : IRequestHandler<EndorsePeerCommand, BaseResponse<bool>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IActivityLogService _activityLogService;

		public EndorsePeerCommandHandler(IUnitOfWork unitOfWork, IActivityLogService activityLogService)
		{
			_unitOfWork = unitOfWork;
			_activityLogService = activityLogService;
		}

		public async Task<BaseResponse<bool>> Handle(EndorsePeerCommand request, CancellationToken cancellationToken)
		{
			if (request.EndorserId == request.EndorseeId)
			{
				throw new BadRequestException("You cannot endorse yourself.");
			}

			// Validate skill exists
			var skill = await _unitOfWork.Skills.GetByIdAsync(request.SkillId);
			if (skill == null) throw new NotFoundException("Skill not found.");

			// Check if already endorsed
			var existingEndorsements = await _unitOfWork.PeerEndorsements.GetAllAsync();
			var alreadyEndorsed = existingEndorsements.Any(e => 
				e.EndorserId == request.EndorserId && 
				e.EndorseeId == request.EndorseeId && 
				e.SkillId == request.SkillId);

			if (alreadyEndorsed)
			{
				throw new ConflictException("You have already endorsed this peer for this skill.");
			}

			var endorsement = new PeerEndorsement
			{
				EndorserId = request.EndorserId,
				EndorseeId = request.EndorseeId,
				SkillId = request.SkillId,
				Comment = request.Comment,
				DateEndorsed = DateTime.UtcNow
			};

			await _unitOfWork.PeerEndorsements.AddAsync(endorsement);

			// Add bonus points to the endorsee
			var endorseeLearner = await _unitOfWork.Learners.GetByIdAsync(request.EndorseeId);
			string endorseeRole;
			if (endorseeLearner != null)
			{
				endorseeLearner.TotalPoints += 10;
				await _unitOfWork.Learners.UpdateAsync(endorseeLearner);
				endorseeRole = Roles.Learner.ToString();
			}
			else
			{
				var endorseeTeamMember = await _unitOfWork.TeamMembers.GetByIdAsync(request.EndorseeId);
				if (endorseeTeamMember != null)
				{
					endorseeTeamMember.TotalPoints += 10;
					await _unitOfWork.TeamMembers.UpdateAsync(endorseeTeamMember);
					endorseeRole = Roles.Team_Members.ToString();
				}
				else
				{
					throw new NotFoundException("Endorsee not found.");
				}
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			await _activityLogService.LogAsync(
				request.EndorseeId,
				endorseeRole,
				UserActivityType.PeerEndorsed,
				$"You were endorsed for the skill '{skill.Name}' by a peer. +10 points.");

			return BaseResponse<bool>.SuccessResponse(true, "Peer endorsed successfully. 10 bonus points have been awarded.");
		}
	}
}

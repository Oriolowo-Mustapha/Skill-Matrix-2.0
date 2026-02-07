using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.RegisterTeamMember
{
	public class CreateTeamMemberCommandHandler : IRequestHandler<CreateTeamMemberCommand, TeamMemberDTO>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailService _emailService;

		public CreateTeamMemberCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_emailService = emailService;
		}
		public async Task<TeamMemberDTO> Handle(CreateTeamMemberCommand request, CancellationToken cancellationToken)
		{
			var getManager = await _unitOfWork.ManagerRepository.GetByIdAsync(request.ManagerId);
			if (getManager == null)
			{
				throw new UnauthorizedException("Unauthorized: No manager found with the provided ID.");
			}
			var getIfEmailExists = await _unitOfWork.TeamMembers.GetByEmailAsync(request.request.Email);
			if (getIfEmailExists != null)
			{
				throw new ConflictException($"Email {request.request.Email} already exists");
			}

			var getOrganization = await _unitOfWork.Organizations.GetByIdAsync(getManager.OrganizationId);
			if (getOrganization == null)
			{
				throw new System.ApplicationException("Could not find the organization associated with the manager.");
			}

			var hashedPassword = HashPassword(request.request.Password);

			var newTeamMember = new TeamMember
			{
				FirstName = request.request.FirstName,
				LastName = request.request.LastName,
				Email = request.request.Email, // 3. Set the email
				UserName = request.request.UserName,
				ProfilePictureUrl = request.request.ProfilePicUrl,
				PasswordHash = hashedPassword,
				Manager = getManager,
				Organization = getOrganization
			};

			await _unitOfWork.TeamMembers.AddAsync(newTeamMember);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var toDTO = new TeamMemberDTO
			{
				Id = newTeamMember.Id,
				FirstName = newTeamMember.FirstName,
				LastName = newTeamMember.LastName,
				Email = newTeamMember.Email,
				UserName = newTeamMember.UserName,
				ProfilePicUrl = newTeamMember.ProfilePictureUrl,
				OrganizationId = newTeamMember.OrganizationId,
				ManagerId = newTeamMember.ManagerId
			};

			var subject = $"You're Invited to Skill Matrix 2.0 - Join {getOrganization.Name}!";
			var body = $"""
				Dear {newTeamMember.UserName},

				You have been invited by {getManager.UserName} to join {getOrganization.Name} on Skill Matrix 2.0.

				Skill Matrix 2.0 is a platform designed to help you track and develop your professional skills.

				To activate your account and set up your password, please click on the following link:
				[Activation Link]

				If you have any questions, please contact your manager, {getManager.UserName}, or our support team.

				We look forward to seeing your progress!

				Best regards,
				The Skill Matrix 2.0 Team
				""";

			await _emailService.SendEmailAsync(newTeamMember.Email, subject, body);
			return toDTO;
		}

		private string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}
	}
}

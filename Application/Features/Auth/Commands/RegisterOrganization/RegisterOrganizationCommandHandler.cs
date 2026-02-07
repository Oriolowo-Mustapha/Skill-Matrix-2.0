using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.RegisterOrganization
{
	public class RegisterOrganizationCommandHandler : IRequestHandler<RegisterOrganizationCommand, OrganizationDTO>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailService _emailService;

		public RegisterOrganizationCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_emailService = emailService;
		}

		public async Task<OrganizationDTO> Handle(RegisterOrganizationCommand command, CancellationToken cancellationToken)
		{
			var request = command.Request;

			var orgExists = await _unitOfWork.Organizations.ExistsAsync(o => o.Name == request.OrganizationName);
			if (orgExists)
			{
				throw new ConflictException($"Organization with name '{request.OrganizationName}' already exists.");
			}
			var managerExists = await _unitOfWork.ManagerRepository.GetByEmailAsync(request.ManagerEmail);
			if (managerExists != null)
			{
				throw new ConflictException($"User with email '{request.ManagerEmail}' already exists.");
			}
			var newOrganization = new Organization
			{
				Name = request.OrganizationName,
				Description = request.OrganizationDescription,
				ProfilePictureUrl = request.OrganizationProfilePictureUrl,
				DateJoined = DateTime.UtcNow
			};

			var hashedPassword = HashPassword(request.ManagerPassword);

			var newManager = new Manager
			{
				FirstName = request.ManagerFirstName,
				LastName = request.ManagerLastName,
				Email = request.ManagerEmail,
				UserName = request.ManagerUserName,
				PasswordHash = hashedPassword,
				Organization = newOrganization
			};

			await _unitOfWork.Organizations.AddAsync(newOrganization);
			await _unitOfWork.ManagerRepository.AddAsync(newManager);

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			var subject = $"Welcome to Skill Matrix 2.0 - {newOrganization.Name}!";
			var body = $"  Dear {newManager.UserName},\r\n\r\nWelcome to Skill Matrix 2.0! Your organization, {newOrganization.Name}, has been\r\nsuccessfully registered.\r\n\r\nSkill Matrix 2.0 helps you assess and track the skills of your team members,\r\n identify areas for improvement, and foster professional growth.\r\n\r\n To get started, you can log in using your registered email and password.\r\n  \r\n If you have any questions or need assistance, please do not hesitate to conta\r\n our support team.\r\n\r\n Best regards,\r\n  The Skill Matrix 2.0 Team";

			await _emailService.SendEmailAsync(newManager.Email, subject, body);

			return new OrganizationDTO
			{
				Id = newOrganization.Id,
				Name = newOrganization.Name,
				Description = newOrganization.Description,
				ProfilePictureUrl = newOrganization.ProfilePictureUrl,
				DateJoined = newOrganization.DateJoined
			};
		}

		private string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}
	}
}

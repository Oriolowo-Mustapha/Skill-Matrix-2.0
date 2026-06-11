using Application.Validators;
using Application.DTOs;
using Application.DTOs.Validators;
using Application.Features.Auth.Commands.RegisterOrganization;
using Application.Features.CareerPaths.Commands.UpdateCareerPathCommand;
using Application.Features.CareerPaths.Commands.UnassignCareerPathFromTeamMemberCommand;
using Application.Features.CareerPaths.Commands.UnassignCareerPathFromLearnerCommand;
using Application.Features.CareerPaths.Commands.DeleteCareerPathCommand;
using Application.Features.CareerPaths.Commands.CreateCareerPathCommand;
using Application.Features.CareerPaths.Commands.AssignCareerPathToTeamMemberCommand;
using Application.Features.CareerPaths.Commands.AssignCareerPathToLearnerCommand;
using Application.Features.Badges.Commands.UpdateBadge;
using Application.Features.Badges.Commands.UnassignBadgeFromTeamMember;
using Application.Features.Badges.Commands.UnassignBadgeFromLearner;
using Application.Features.Badges.Commands.DeleteBadge;
using Application.Features.Badges.Commands.CreateBadge;
using Application.Features.Badges.Commands.AssignBadgeToTeamMember;
using Application.Features.Badges.Commands.AssignBadgeToLearner;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
	public static class ValidatorExtensions
	{
		public static IServiceCollection AddManualValidators(this IServiceCollection services)
		{
			services.AddScoped<IValidator<UpdateUserRequestDTO>, UpdateUserRequestDTOValidator>();
			services.AddScoped<IValidator<RegisterTeamMemberRequestDTO>, RegisterTeamMemberRequestDTOValidator>();
			services.AddScoped<IValidator<RegisterOrganizationCommand>, RegisterOrganizationCommandValidator>();
			services.AddScoped<IValidator<AssesmentDTO>, AssesmentDTOValidator>();
			
			// Career Path Validators
			services.AddScoped<IValidator<UpdateCareerPathCommand>, UpdateCareerPathCommandValidator>();
			services.AddScoped<IValidator<UnassignCareerPathFromTeamMemberCommand>, UnassignCareerPathFromTeamMemberCommandValidator>();
			services.AddScoped<IValidator<UnassignCareerPathFromLearnerCommand>, UnassignCareerPathFromLearnerCommandValidator>();
			services.AddScoped<IValidator<DeleteCareerPathCommand>, DeleteCareerPathCommandValidator>();
			services.AddScoped<IValidator<CreateCareerPathCommand>, CreateCareerPathCommandValidator>();
			services.AddScoped<IValidator<AssignCareerPathToTeamMemberCommand>, AssignCareerPathToTeamMemberCommandValidator>();
			services.AddScoped<IValidator<AssignCareerPathToLearnerCommand>, AssignCareerPathToLearnerCommandValidator>();

			// Badge Validators
			services.AddScoped<IValidator<UpdateBadgeCommand>, UpdateBadgeCommandValidator>();
			services.AddScoped<IValidator<UnassignBadgeFromTeamMemberCommand>, UnassignBadgeFromTeamMemberCommandValidator>();
			services.AddScoped<IValidator<UnassignBadgeFromLearnerCommand>, UnassignBadgeFromLearnerCommandValidator>();
			services.AddScoped<IValidator<DeleteBadgeCommand>, DeleteBadgeCommandValidator>();
			services.AddScoped<IValidator<CreateBadgeCommand>, CreateBadgeCommandValidator>();
			services.AddScoped<IValidator<AssignBadgeToTeamMemberCommand>, AssignBadgeToTeamMemberCommandValidator>();
			services.AddScoped<IValidator<AssignBadgeToLearnerCommand>, AssignBadgeToLearnerCommandValidator>();

			return services;
		}
	}
}

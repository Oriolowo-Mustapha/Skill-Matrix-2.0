using Application.Features.CareerPaths.Commands.GenerateAiCatalog;
using Application.Features.Skills.Commands;
using Application.Interfaces.Repository;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skill_Matrix_2_0.BackgroundServices
{
	public class CareerPathSeedService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<CareerPathSeedService> _logger;

		public CareerPathSeedService(
			IServiceScopeFactory scopeFactory,
			ILogger<CareerPathSeedService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			// Delay startup by 12 seconds to ensure SkillCatalogSeedService has time to populate skills if empty
			await Task.Delay(TimeSpan.FromSeconds(12), stoppingToken);

			_logger.LogInformation("CareerPathSeedService: Running startup career path & tracks catalog check...");

			try
			{
				using var scope = _scopeFactory.CreateScope();
				var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
				var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

				var skills = (await unitOfWork.Skills.GetAllAsync()).ToList();

				if (!skills.Any())
				{
					_logger.LogInformation("CareerPathSeedService: No skills found in database. Triggering skill catalog generation first...");
					var skillResult = await mediator.Send(new GenerateAiSkillCatalogCommand(), stoppingToken);
					_logger.LogInformation("CareerPathSeedService: Skill catalog generation completed: {Message}", skillResult?.Message);

					skills = (await unitOfWork.Skills.GetAllAsync()).ToList();
				}

				if (!skills.Any())
				{
					_logger.LogWarning("CareerPathSeedService: Skill catalog generation did not populate skills. Skipping career path seeding.");
					return;
				}

				var careerPaths = (await unitOfWork.CareerPaths.GetAllAsync()).ToList();
				var mappedPathSkills = (await unitOfWork.CareerPathSkills.GetAllAsync()).ToList();

				var mappedSkillIds = mappedPathSkills.Select(cps => cps.SkillId).ToHashSet();
				var unmappedSkills = skills.Where(s => !mappedSkillIds.Contains(s.Id)).ToList();

				bool needsSeeding = !careerPaths.Any();
				bool hasUnmappedSkills = unmappedSkills.Any();

				if (!needsSeeding && !hasUnmappedSkills)
				{
					_logger.LogInformation(
						"CareerPathSeedService: Catalog fully populated ({PathCount} paths, {TrackSkillCount} mapped skills, 0 unmapped). No action needed.",
						careerPaths.Count, mappedPathSkills.Count);
					return;
				}

				if (needsSeeding)
				{
					_logger.LogInformation("CareerPathSeedService: Career paths table is empty. Triggering full AI career path generation...");
				}
				else if (hasUnmappedSkills)
				{
					_logger.LogInformation(
						"CareerPathSeedService: Detected {Count} untracked skills in database. Triggering incremental AI career path mapping...",
						unmappedSkills.Count);
				}

				var result = await mediator.Send(new GenerateAiCatalogCommand(), stoppingToken);

				if (result != null)
				{
					_logger.LogInformation("CareerPathSeedService: AI career path catalog sync completed. {Message}", result.Message);
				}
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("CareerPathSeedService: Career path seeding task was cancelled.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "CareerPathSeedService: Failed to seed career path catalog on startup.");
			}
		}
	}
}

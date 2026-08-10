using Application.Features.Skills.Commands;
using Application.Interfaces.Repository;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Skill_Matrix_2_0.BackgroundServices
{
	public class SkillCatalogSeedService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<SkillCatalogSeedService> _logger;

		public SkillCatalogSeedService(
			IServiceScopeFactory scopeFactory,
			ILogger<SkillCatalogSeedService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			// Delay startup to let other services initialize
			await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

			_logger.LogInformation("SkillCatalogSeedService: Running startup skill catalog check...");

			try
			{
				using var scope = _scopeFactory.CreateScope();
				var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
				var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

				var existingSkills = await unitOfWork.Skills.GetAllAsync();

				if (existingSkills.Any())
				{
					_logger.LogInformation(
						$"SkillCatalogSeedService: Skills table already populated with {existingSkills.Count()} skills. No action needed.");
					return;
				}

				_logger.LogInformation("SkillCatalogSeedService: Skills table is empty. Triggering AI catalog generation in background...");

				var result = await mediator.Send(new GenerateAiSkillCatalogCommand(), stoppingToken);

				if (result != null && result.Success)
				{
					_logger.LogInformation($"SkillCatalogSeedService: AI skill catalog seeded successfully. {result.Message}");
				}
				else
				{
					_logger.LogWarning($"SkillCatalogSeedService: AI skill catalog generation did not succeed. {result?.Message}");
				}
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("SkillCatalogSeedService: Startup seeding was cancelled.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "SkillCatalogSeedService: Failed to seed skill catalog on startup.");
			}
		}
	}
}

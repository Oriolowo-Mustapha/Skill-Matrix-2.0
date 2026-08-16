using Application.Features.CareerPaths.Commands.GenerateAiCatalog;
using Application.Features.Skills.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.EventHandlers
{
    public class SyncCareerPathsOnSkillAddedHandler : INotificationHandler<SkillsAddedNotification>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SyncCareerPathsOnSkillAddedHandler> _logger;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public SyncCareerPathsOnSkillAddedHandler(
            IMediator mediator,
            ILogger<SyncCareerPathsOnSkillAddedHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(SkillsAddedNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "SyncCareerPathsOnSkillAddedHandler: Received SkillsAddedNotification ({Count} skill(s) added via {Source}).",
                notification.AddedSkillNames.Count, notification.Source);

            // Coalesce concurrent calls so multiple skill additions don't spam OpenRouter API simultaneously
            if (!await _semaphore.WaitAsync(0, cancellationToken))
            {
                _logger.LogInformation("SyncCareerPathsOnSkillAddedHandler: Career path sync is already running in background. Skipping duplicate trigger.");
                return;
            }

            try
            {
                _logger.LogInformation("SyncCareerPathsOnSkillAddedHandler: Triggering AI career path catalog generation for new skills...");
                var result = await _mediator.Send(new GenerateAiCatalogCommand(), cancellationToken);

                if (result != null)
                {
                    _logger.LogInformation("SyncCareerPathsOnSkillAddedHandler: Career path catalog updated: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncCareerPathsOnSkillAddedHandler: Failed to sync career paths for newly added skills.");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}

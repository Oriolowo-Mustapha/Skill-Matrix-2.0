using Application.DTOs;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Skills.Commands
{
    public class GenerateAiSkillCatalogCommandHandler : IRequestHandler<GenerateAiSkillCatalogCommand, BaseResponse<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiService _aiService;
        private readonly IMediator _mediator;
        private readonly ILogger<GenerateAiSkillCatalogCommandHandler> _logger;

        public GenerateAiSkillCatalogCommandHandler(
            IUnitOfWork unitOfWork, 
            IAiService aiService,
            IMediator mediator,
            ILogger<GenerateAiSkillCatalogCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _aiService = aiService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<BaseResponse<string>> Handle(GenerateAiSkillCatalogCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting AI skill catalog generation...");
                var aiSkills = await _aiService.GenerateSkillCatalogAsync();

                if (aiSkills == null || !aiSkills.Any())
                {
                    return new BaseResponse<string>
                    {
                        Success = false,
                        Message = "AI did not return any skills. Please try again.",
                        Data = null
                    };
                }

                var existingSkills = (await _unitOfWork.Skills.GetAllAsync()).ToList();
                int addedCount = 0;
                int skippedCount = 0;
                var addedSkillNames = new List<string>();

                foreach (var aiSkill in aiSkills)
                {
                    if (string.IsNullOrWhiteSpace(aiSkill.Name)) continue;

                    var cleanName = aiSkill.Name.Trim();
                    var exists = existingSkills.Any(s =>
                        s.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase));

                    if (!exists)
                    {
                        var newSkill = new Skill
                        {
                            Id = Guid.NewGuid(),
                            ExternalId = $"ai-generated-{Guid.NewGuid():N}",
                            Name = cleanName,
                            Category = !string.IsNullOrWhiteSpace(aiSkill.Category) ? aiSkill.Category.Trim() : "General Skills",
                            Source = "AI-Generated",
                            IsCustomized = false,
                            DateAdded = DateTime.UtcNow
                        };

                        await _unitOfWork.Skills.AddAsync(newSkill);
                        existingSkills.Add(newSkill);
                        addedSkillNames.Add(cleanName);
                        addedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }

                if (addedCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Publish batch notification so career paths generate automatically for new skills
                    await _mediator.Publish(new Notifications.SkillsAddedNotification(addedSkillNames, "AI-Generated"), cancellationToken);
                }

                _logger.LogInformation("AI skill catalog generation complete. Added: {Added}, Skipped: {Skipped}", addedCount, skippedCount);

                return new BaseResponse<string>
                {
                    Success = true,
                    Message = $"Successfully generated {addedCount} new skills via AI. ({skippedCount} duplicates skipped)",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate AI skill catalog.");
                return new BaseResponse<string>
                {
                    Success = false,
                    Message = $"AI Skill Catalog generation failed: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}

using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Repository;
using Application.Features.Skills.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Assessments.Queries.GetSkills
{
	public class GetSkillsQueryHandler : IRequestHandler<GetSkillsQuery, BaseResponse<List<SkillDTO>>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMediator _mediator;
		private readonly ILogger<GetSkillsQueryHandler> _logger;

		public GetSkillsQueryHandler(
			IUnitOfWork unitOfWork, 
			IMediator mediator,
			ILogger<GetSkillsQueryHandler> logger)
		{
			_unitOfWork = unitOfWork;
			_mediator = mediator;
			_logger = logger;
		}

		public async Task<BaseResponse<List<SkillDTO>>> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
		{
			var skills = await _unitOfWork.Skills.GetAllAsync();
			if (!skills.Any())
			{
				try
				{
					_logger.LogInformation("Skills table is empty. Triggering AI skill catalog generation...");
					var syncResult = await _mediator.Send(new GenerateAiSkillCatalogCommand(), cancellationToken);
					if (syncResult != null && syncResult.Success)
					{
						skills = await _unitOfWork.Skills.GetAllAsync();
						_logger.LogInformation("AI skill catalog populated successfully with {Count} skills.", skills.Count());
					}
					else
					{
						_logger.LogWarning("AI skill catalog generation did not return skills: {Message}", syncResult?.Message);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "An error occurred while attempting AI skill catalog auto-generation.");
				}
			}

			var dtoList = skills.Any() ? skills.ToSkillDTOList() : new List<SkillDTO>();
			string message = skills.Any() 
				? "Skills retrieved successfully." 
				: "No skills available in catalog. Skills will be auto-generated via AI when the catalog is first accessed. Please retry in a moment.";

			return BaseResponse<List<SkillDTO>>.SuccessResponse(dtoList, message);
		}
	}
}

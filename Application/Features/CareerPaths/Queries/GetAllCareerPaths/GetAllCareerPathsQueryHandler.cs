using Application.DTOs;
using Application.Extensions;
using Application.Features.CareerPaths.Commands.GenerateAiCatalog;
using Application.Interfaces.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CareerPaths.Queries.GetAllCareerPaths
{
	public class GetAllCareerPathsQueryHandler : IRequestHandler<GetAllCareerPathsQuery, List<CareerPathDTO>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMediator _mediator;
		private readonly ILogger<GetAllCareerPathsQueryHandler> _logger;

		public GetAllCareerPathsQueryHandler(
			IUnitOfWork unitOfWork,
			IMediator mediator,
			ILogger<GetAllCareerPathsQueryHandler> logger)
		{
			_unitOfWork = unitOfWork;
			_mediator = mediator;
			_logger = logger;
		}

		public async Task<List<CareerPathDTO>> Handle(GetAllCareerPathsQuery request, CancellationToken cancellationToken)
		{
			var careerPaths = await _unitOfWork.CareerPaths.GetAllAsync(
				cp => cp.CareerPathSkills,
				cp => cp.Tracks);

			if (!careerPaths.Any())
			{
				var skills = await _unitOfWork.Skills.GetAllAsync();
				if (skills.Any())
				{
					try
					{
						_logger.LogInformation("Career paths table is empty but skills exist. Triggering AI catalog generation...");
						await _mediator.Send(new GenerateAiCatalogCommand(), cancellationToken);
						careerPaths = await _unitOfWork.CareerPaths.GetAllAsync(
							cp => cp.CareerPathSkills,
							cp => cp.Tracks);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Failed to auto-generate career path catalog during query.");
					}
				}
			}

			var pathIds = careerPaths.Select(cp => cp.Id).ToList();
			if (pathIds.Any())
			{
				await _unitOfWork.CareerPathSkills.FindAsync(
					cps => pathIds.Contains(cps.CareerPathId),
					cps => cps.Skill
				);
			}

			return careerPaths.OrderBy(cp => cp.Title).ToDtoList();
		}
	}
}
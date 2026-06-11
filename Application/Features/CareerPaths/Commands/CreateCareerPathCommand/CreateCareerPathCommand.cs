using Application.DTOs;
﻿using MediatR;

namespace Application.Features.CareerPaths.Commands.CreateCareerPathCommand
{
	public record CreateCareerPathCommand(
		string Title,
		string Description,
		string IconURL) : IRequest<Guid>;
}

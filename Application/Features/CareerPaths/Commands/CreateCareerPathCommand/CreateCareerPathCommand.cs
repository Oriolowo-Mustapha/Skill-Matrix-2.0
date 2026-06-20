using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace Application.Features.CareerPaths.Commands.CreateCareerPathCommand
{
	public record CreateCareerPathCommand(
		string Title,
		string Description,
		IFormFile? Icon) : IRequest<BaseResponse<Guid>>;
}


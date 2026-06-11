using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.CareerPaths.Commands.DeleteCareerPathCommand
{
    public record DeleteCareerPathCommand(Guid Id) : IRequest;
}

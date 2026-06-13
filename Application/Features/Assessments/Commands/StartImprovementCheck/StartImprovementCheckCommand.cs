using Application.DTOs;
using MediatR;
using System;

namespace Application.Features.Assessments.Commands.StartImprovementCheck
{
	public record StartImprovementCheckCommand(Guid SkillId, string Concept, Guid UserId, string UserRole) : IRequest<StartAssessmentResponseDTO>;
}

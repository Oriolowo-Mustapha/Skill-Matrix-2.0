using Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Assessments.Commands.StartTrackBaseline
{
    public class StartTrackBaselineCommand : IRequest<BaseResponse<List<StartAssessmentResponseDTO>>>
    {
        public StartTrackBaselineRequestDTO Dto { get; set; } = null!;
        public Guid UserId { get; set; }
        public string UserRole { get; set; } = string.Empty;
    }
}

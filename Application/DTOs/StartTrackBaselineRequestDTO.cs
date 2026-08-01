using Domain.Enum;
using System;

namespace Application.DTOs
{
    public class StartTrackBaselineRequestDTO
    {
        public Guid CareerPathTrackId { get; set; }
        public ProficiencyLevel DeclaredProficiencyLevel { get; set; }
    }
}

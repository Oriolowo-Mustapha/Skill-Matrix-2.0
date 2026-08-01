using Domain.Enum;

namespace Application.DTOs.Ai
{
    public class GeneratedTrackSkillDto
    {
        public string SkillName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProficiencyLevel TargetLevel { get; set; }
    }
}

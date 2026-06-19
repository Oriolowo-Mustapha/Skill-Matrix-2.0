using System;
using Domain.Entities;

namespace Domain.Entities
{
    public class CareerPathSkill : BaseEntity
    {
        public Guid CareerPathId { get; set; }
        public CareerPath CareerPath { get; set; } = null!;

        public Guid SkillId { get; set; }
        public Skill Skill { get; set; } = null!;

        // Nullable: null = Core skill (everyone gets it), non-null = Track-specific skill
        public Guid? CareerPathTrackId { get; set; }
        public CareerPathTrack? CareerPathTrack { get; set; }

        public Domain.Enum.ProficiencyLevel TargetLevel { get; set; } = Domain.Enum.ProficiencyLevel.Novice;
    }
}
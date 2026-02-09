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
    }
}
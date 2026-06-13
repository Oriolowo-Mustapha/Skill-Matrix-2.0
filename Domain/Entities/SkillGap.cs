using System;

namespace Domain.Entities
{
	public class SkillGap : BaseEntity
	{
		public Guid? LearnerId { get; set; }
		public Learner? Learner { get; set; }
		public Guid? TeamMemberId { get; set; }
		public TeamMember? TeamMember { get; set; }
		public Guid SkillId { get; set; } // Points to AssignedSkill
		public AssignedSkill Skill { get; set; } = null!;
		public Guid AssessmentResultId { get; set; }
		public AssessmentResult AssessmentResult { get; set; } = null!;
		public string Concept { get; set; } = string.Empty;
		public int Score { get; set; }
		public DateTime DateIdentified { get; set; } = DateTime.UtcNow;
		public string Status { get; set; } = "Active"; // "Active", "Resolved"
	}
}

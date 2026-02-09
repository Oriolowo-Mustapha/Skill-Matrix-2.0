namespace Domain.Entities
{
	public class Skill : BaseEntity
	{
		public string Name { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public DateTime DateAdded { get; set; } = DateTime.UtcNow;
		public List<AssignedSkill> AssignedSkills { get; set; } = new List<AssignedSkill>();

        // Navigation property for the many-to-many relationship with CareerPath
        public ICollection<CareerPathSkill> CareerPathSkills { get; set; } = new List<CareerPathSkill>();
	}
}
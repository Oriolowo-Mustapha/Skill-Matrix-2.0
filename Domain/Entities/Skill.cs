namespace Domain.Entities
{
	public class Skill : BaseEntity
	{
		public string Name { get; set; }
		public string Category { get; set; }
		public DateTime DateAdded { get; set; }
		public List<AssignedSkill> AssignedSkills { get; set; }
	}
}

namespace Domain.Entities
{
	public class CareerPathTrack : BaseEntity
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string? IconUrl { get; set; }
		public Guid CareerPathId { get; set; }
		public CareerPath CareerPath { get; set; } = null!;
		public ICollection<CareerPathSkill> CareerPathSkills { get; set; } = new List<CareerPathSkill>();
	}
}

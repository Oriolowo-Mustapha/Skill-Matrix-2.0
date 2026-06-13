namespace Domain.Entities
{
	public class PeerEndorsement : BaseEntity
	{
		public Guid EndorserId { get; set; }
		public Guid EndorseeId { get; set; }
		public Guid SkillId { get; set; }
		public string Comment { get; set; } = string.Empty;
		public DateTime DateEndorsed { get; set; } = DateTime.UtcNow;
	}
}

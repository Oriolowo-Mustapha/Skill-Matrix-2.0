using Domain.Enum;

namespace Domain.Entities
{
	public class UserActivityLog : BaseEntity
	{
		public Guid UserId { get; set; }
		public string UserRole { get; set; } = string.Empty;
		public UserActivityType ActivityType { get; set; }
		public string Description { get; set; } = string.Empty;
		public string? EntityType { get; set; }
		public Guid? EntityId { get; set; }
		public int PointsEarned { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
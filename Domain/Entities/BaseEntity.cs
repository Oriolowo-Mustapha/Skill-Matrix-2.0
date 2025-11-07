using MassTransit;

namespace Domain.Entities
{
	public class BaseEntity
	{
		public Guid Id { get; set; } = NewId.Next().ToGuid();
	}
}
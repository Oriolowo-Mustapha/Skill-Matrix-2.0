using MassTransit;

namespace Domain.Entities
{
	public class BaseEntity
	{
		public Guid Id { get; private set; } = NewId.Next().ToGuid();
	}
}

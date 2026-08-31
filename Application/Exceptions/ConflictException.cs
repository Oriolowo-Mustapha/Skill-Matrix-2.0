namespace Application.Exceptions
{
	public class ConflictException : ApplicationException
	{
		public ConflictException()
			: base("This action conflicts with the current state of the resource. Please refresh and try again.")
		{
		}

		public ConflictException(string message)
			: base(message)
		{
		}

		public ConflictException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
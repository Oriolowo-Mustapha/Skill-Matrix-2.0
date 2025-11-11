namespace Application.Exceptions
{
	public class ConflictException : ApplicationException
	{
		public ConflictException()
			: base("A conflict occurred with the current state of the resource.")
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
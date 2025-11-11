namespace Application.Exceptions
{
	public class ForbiddenException : ApplicationException
	{
		public ForbiddenException()
			: base("You are not authorized to perform this action.")
		{
		}

		public ForbiddenException(string message)
			: base(message)
		{
		}

		public ForbiddenException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
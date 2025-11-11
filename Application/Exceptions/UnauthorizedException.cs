namespace Application.Exceptions
{
	public class UnauthorizedException : ApplicationException
	{
		public UnauthorizedException() : base("Authentication Failed")
		{

		}

		public UnauthorizedException(string message) : base(message)
		{
		}

		public UnauthorizedException
			(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}

namespace Application.Exceptions
{
	public class UnauthorizedException : ApplicationException
	{
		public UnauthorizedException() : base("Authentication failed. Please verify your credentials and try again.")
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

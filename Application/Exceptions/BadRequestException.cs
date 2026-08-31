namespace Application.Exceptions
{
	public class BadRequestException : ApplicationException
	{
		public BadRequestException() : base("The request is invalid. Please check your input and try again.")
		{
		}

		public BadRequestException(string message) : base(message)
		{
		}

		public BadRequestException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}

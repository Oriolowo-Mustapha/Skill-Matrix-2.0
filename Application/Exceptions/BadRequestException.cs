namespace Application.Exceptions
{
	public class BadRequestException : ApplicationException
	{
		public BadRequestException() : base("The request is Invalid")
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

namespace Application.Exceptions
{
	public class AiServiceException : ApplicationException
	{
		public AiServiceException()
			: base("The AI service is temporarily unavailable. Please try again later.")
		{
		}

		public AiServiceException(string message)
			: base(message)
		{
		}

		public AiServiceException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
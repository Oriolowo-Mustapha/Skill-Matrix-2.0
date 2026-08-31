namespace Application.Exceptions
{
	public class NotFoundException : ApplicationException
	{
		public NotFoundException() : base()
		{
		}

		public NotFoundException(string message) : base(message)
		{

		}

		public NotFoundException(string message, Exception innerException) : base(message, innerException)
		{

		}

		public NotFoundException(string entityName, object key) : base($"{entityName} with identifier \"{key}\" was not found.")
		{

		}
	}
}

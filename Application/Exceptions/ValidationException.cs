namespace Application.Exceptions
{
	public class ValidationException : BadRequestException
	{
		public IDictionary<string, string[]> Errors { get; }

		public ValidationException() : base("One or more validation errors occurred. Please review the highlighted fields.")
		{
			Errors = new Dictionary<string, string[]>();
		}

		public ValidationException(IDictionary<string, string[]> failures) : this()
		{
			Errors = failures;
		}

	}
}

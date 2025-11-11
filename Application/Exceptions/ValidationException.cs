namespace Application.Exceptions
{
	public class ValidationException : BadRequestException
	{
		public IDictionary<string, string[]> Errors { get; }

		public ValidationException() : base("One Or More Validations Failure Has Occured")
		{
			Errors = new Dictionary<string, string[]>();
		}

		public ValidationException(IDictionary<string, string[]> failures) : this()
		{
			Errors = failures;
		}

	}
}

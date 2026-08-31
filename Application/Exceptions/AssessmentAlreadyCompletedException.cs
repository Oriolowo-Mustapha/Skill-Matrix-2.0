namespace Application.Exceptions
{
	public class AssessmentAlreadyCompletedException : BadRequestException
	{
		private const string DefaultMessage = "This assessment has already been completed and cannot be resubmitted.";
	}
}

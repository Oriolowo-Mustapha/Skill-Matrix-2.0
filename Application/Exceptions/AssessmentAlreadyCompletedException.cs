namespace Application.Exceptions
{
	public class AssessmentAlreadyCompletedException : BadRequestException
	{
		private const string DefaultMessage = "This Assessment Has Already been Completed abd cant be submitted again";
	}
}

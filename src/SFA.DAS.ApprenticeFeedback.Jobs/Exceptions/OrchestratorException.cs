namespace SFA.DAS.ApprenticeFeedback.Jobs.Exceptions
{
    public class OrchestratorException : Exception
    {
        public OrchestratorException() 
        { 
        }

        public OrchestratorException(string? message)
            : base(message)
        {

        }

        public OrchestratorException(string? message, Exception? innerException)
            : base(message, innerException)
        {

        }
    }
}

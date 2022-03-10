using NServiceBus;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure
{
    public static class SendLocally
    {
        public static SendOptions Options
        {
            get
            {
                var options = new SendOptions();
                options.RouteToThisEndpoint();
                return options;
            }
        }
    }
}
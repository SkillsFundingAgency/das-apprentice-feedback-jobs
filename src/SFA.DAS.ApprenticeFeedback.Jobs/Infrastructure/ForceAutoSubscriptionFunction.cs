using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure
{
    public class ForceAutoEventSubscription : IMessage { }

    public class ForceAutoEventSubscriptionFunction(IFunctionEndpoint functionEndpoint)
    {
        [Function("ForceAutoSubscriptionFunction")]
        public async Task Run([TimerTrigger("* * * 1 1 *", RunOnStartup = true)] TimerInfo myTimer,
            ILogger logger, FunctionContext executionContext)
        {
            var sendOptions = SendLocally.Options;
            sendOptions.SetHeader(Headers.ControlMessageHeader, bool.TrueString);
            sendOptions.SetHeader(Headers.MessageIntent, "Send");
            await functionEndpoint.Send(new ForceAutoEventSubscription(), sendOptions, executionContext);
        }
    }

    public class ForceAutoEventSubscriptionHandler : IHandleMessages<ForceAutoEventSubscription>
    {
        public Task Handle(ForceAutoEventSubscription message, IMessageHandlerContext context)
            => Task.CompletedTask;
    }
}
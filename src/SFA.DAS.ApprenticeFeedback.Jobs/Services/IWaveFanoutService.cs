using Microsoft.DurableTask;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Services
{
    public interface IWaveFanoutService
    {
        Task<IReadOnlyList<TOut>> ExecuteAsync<TIn, TOut>(TaskOrchestrationContext ctx, IEnumerable<TIn> items, 
            Func<TaskOrchestrationContext, TIn, Task<TOut>> startFunc,
            Func<TaskOrchestrationContext, Task> delayFunc);
    }
}
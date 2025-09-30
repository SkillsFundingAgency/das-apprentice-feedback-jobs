using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Services
{
    /// <summary>
    /// Wave-based fan-out: start up to N items, await all completions, then wait 1s, repeat.
    /// Deterministic for Durable Task (uses only ctx time & CreateTimer).
    /// </summary>
    public class WaveFanoutService : IWaveFanoutService
    {
        private readonly int _perSecondCap;

        public WaveFanoutService(int perSecondCap)
        {
            if (perSecondCap <= 0) throw new ArgumentOutOfRangeException(nameof(perSecondCap));
            _perSecondCap = perSecondCap;
        }

        public async Task<IReadOnlyList<TOut>> ExecuteAsync<TIn, TOut>(
            TaskOrchestrationContext ctx,
            IEnumerable<TIn> items,
            Func<TaskOrchestrationContext, TIn, Task<TOut>> startFunc,
            Func<TaskOrchestrationContext, Task> delayFunc)
        {
            ArgumentNullException.ThrowIfNull(ctx);
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(startFunc);
            ArgumentNullException.ThrowIfNull(delayFunc);

            var log = ctx.CreateReplaySafeLogger("WaveFanOut");

            var list = (items as IList<TIn>) ?? items.ToList();
            var results = new List<TOut>(list.Count);

            log.LogDebug("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Activities to process {ActivityCount}, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, list.Count, ctx.IsReplaying);

            int index = 0;
            while (index < list.Count)
            {
                int remaining = list.Count - index;
                int take = Math.Min(_perSecondCap, remaining);

                var waveTasks = new List<Task<TOut>>(take);
                for (int k = 0; k < take; k++)
                {
                    waveTasks.Add(startFunc(ctx, list[index + k]));
                }

                log.LogDebug("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Activities tasks to wait for {TaskCount}, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, waveTasks.Count, ctx.IsReplaying);

                var waveResults = await Task.WhenAll(waveTasks);
                results.AddRange(waveResults);
                index += take;

                if (index < list.Count)
                {
                    log.LogDebug("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Waiting, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, ctx.IsReplaying);

                    await delayFunc(ctx);

                    log.LogDebug("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Resumed, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, ctx.IsReplaying);
                }
            }

            log.LogDebug("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Results to report {ResultCount}, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, results.Count, ctx.IsReplaying);

            return results;
        }
    }
}

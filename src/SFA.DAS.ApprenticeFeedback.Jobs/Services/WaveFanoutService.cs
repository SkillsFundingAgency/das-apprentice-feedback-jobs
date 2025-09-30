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
            Action<TaskOrchestrationContext> delayAction)
        {
            ArgumentNullException.ThrowIfNull(ctx);
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(startFunc);

            var log = ctx.CreateReplaySafeLogger("WaveFanOut");

            var list = (items as IList<TIn>) ?? items.ToList();
            var results = new List<TOut>(list.Count);

            log.LogInformation("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Activities to process {ActivityCount}, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, list.Count, ctx.IsReplaying);

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

                log.LogInformation("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Activities tasks to wait for {TaskCount}, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, waveTasks.Count, ctx.IsReplaying);

                var waveResults = await Task.WhenAll(waveTasks);
                results.AddRange(waveResults);
                index += take;

                if (index < list.Count)
                {
                    log.LogInformation("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Waiting, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, ctx.IsReplaying);

                    delayAction(ctx);

                    log.LogInformation("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Resumed, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, ctx.IsReplaying);

                    //var resumeAt = ctx.CurrentUtcDateTime.AddSeconds(5);

                    //log.LogInformation("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Waiting until {ResumeAt} to continue processing, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, resumeAt, ctx.IsReplaying);

                    //await ctx.CreateTimer(resumeAt, CancellationToken.None);

                    //log.LogInformation("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Resumed, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, ctx.IsReplaying);
                }
            }

            log.LogInformation("WaveFanOut {InstanceId}@{CurrentUtcDateTime}: Results to report {ResultCount}, replaying {Replaying}", ctx.InstanceId, ctx.CurrentUtcDateTime, results.Count, ctx.IsReplaying);

            return results;
        }
    }
}

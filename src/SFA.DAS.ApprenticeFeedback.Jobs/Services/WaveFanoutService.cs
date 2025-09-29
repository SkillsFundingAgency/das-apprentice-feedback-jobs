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
        private readonly ILogger<WaveFanoutService> _logger;

        public WaveFanoutService(int perSecondCap, ILogger<WaveFanoutService> logger)
        {
            if (perSecondCap <= 0) throw new ArgumentOutOfRangeException(nameof(perSecondCap));
            _perSecondCap = perSecondCap;

            _logger = logger;
        }

        public async Task<IReadOnlyList<TOut>> ExecuteAsync<TIn, TOut>(
            TaskOrchestrationContext ctx,
            IEnumerable<TIn> items,
            Func<TaskOrchestrationContext, TIn, Task<TOut>> startFunc)
        {
            ArgumentNullException.ThrowIfNull(ctx);
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(startFunc);

            var list = (items as IList<TIn>) ?? items.ToList();

            _logger.LogInformation("WaveFanOut: Activities to process {ActivityCount}", list.Count);

            var results = new List<TOut>(list.Count);

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

                _logger.LogInformation("WaveFanOut: Activities tasks to wait for {TaskCount}", waveTasks.Count);

                var waveResults = await Task.WhenAll(waveTasks);
                results.AddRange(waveResults);
                index += take;

                if (index < list.Count)
                {
                    var resumeAt = ctx.CurrentUtcDateTime.AddSeconds(1);

                    _logger.LogInformation("WaveFanOut: Waiting until {ResumeAt} to continue processing", resumeAt);

                    await ctx.CreateTimer(resumeAt, CancellationToken.None);
                }
            }

            _logger.LogInformation("WaveFanOut: Results to report {ResultCount}", results.Count);

            return results;
        }
    }
}

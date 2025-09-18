using Microsoft.DurableTask;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Services
{
    /// <summary>
    /// A replay-safe sliding-window fan-out that enforces a cap on starts per rolling second.
    /// </summary>
    public sealed class SlidingWindowFanoutService
    {
        private readonly int _perSecondCap;

        public SlidingWindowFanoutService(int perSecondCap)
        {
            _perSecondCap = perSecondCap;
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
            var results = new List<TOut>(list.Count);

            // start times within the last 1 second window, in chronological order.
            var starts = new Queue<DateTime>();
            var window = TimeSpan.FromSeconds(1);

            int index = 0;
            while (index < list.Count)
            {
                var now = ctx.CurrentUtcDateTime;

                // remove at/before the window threshold
                var threshold = now - window;
                while (starts.Count > 0 && starts.Peek() <= threshold)
                    starts.Dequeue();

                int used = starts.Count;
                int available = Math.Max(0, _perSecondCap - used);

                if (available <= 0)
                {
                    // oldest start inside the window will age out at this time
                    var freeAt = starts.Peek().Add(window);
                    if (freeAt <= now) freeAt = now.AddMilliseconds(1);
                    await ctx.CreateTimer(freeAt, CancellationToken.None);
                    continue;
                }

                int remaining = list.Count - index;
                int take = Math.Min(available, remaining);

                // recording the start times before starting so subsequent iterations see them inside the window
                for (int takeIndex = 0; takeIndex < take; takeIndex++) 
                    starts.Enqueue(now);

                var batch = new List<Task<TOut>>(take);
                for (int k = 0; k < take; k++)
                    batch.Add(startFunc(ctx, list[index + k]));

                var batchResults = await Task.WhenAll(batch);
                results.AddRange(batchResults);
                index += take;
            }

            return results;
        }
    }
}
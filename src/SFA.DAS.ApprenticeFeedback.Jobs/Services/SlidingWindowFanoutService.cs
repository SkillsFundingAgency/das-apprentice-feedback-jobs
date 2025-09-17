using Microsoft.DurableTask;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Services
{
    public sealed class SlidingWindowFanoutService
    {
        private readonly int _perSecondCap;
        private readonly int _perMinuteCap;

        public SlidingWindowFanoutService(int perSecondCap, int perMinuteCap)
        {
            _perSecondCap = perSecondCap;
            _perMinuteCap = perMinuteCap;
        }

        /// <summary>
        /// Executes a sliding-window rate-limited fan-out inside a Durable orchestrator.
        /// </summary>
        /// <typeparam name="TIn">Item type</typeparam>
        /// <typeparam name="TOut">Result type</typeparam>
        /// <param name="ctx">Durable orchestration context</param>
        /// <param name="items">Items to process</param>
        /// <param name="startFunc">Delegate that starts work for a single item (e.g., CallActivityAsync)</param>
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
            var startsInLastMinute = new Queue<DateTime>();

            int index = 0;
            while (index < list.Count)
            {
                var now = ctx.CurrentUtcDateTime;

                DequeueOlderThan(startsInLastMinute, now - TimeSpan.FromMinutes(1));

                int usedLastMinute = startsInLastMinute.Count;
                int usedLastSecond = CountSince(startsInLastMinute, now);

                int availPerMinute = _perMinuteCap - usedLastMinute;
                int availPerSecond = _perSecondCap - usedLastSecond;
                int available = Math.Max(0, Math.Min(availPerSecond, availPerMinute));

                if (available <= 0)
                {
                    // determine whether to wait for the next second or the next minute
                    var nextSecond = NextFreeTime(startsInLastMinute, TimeSpan.FromSeconds(1), _perSecondCap, now);
                    var nextMinute = NextFreeTime(startsInLastMinute, TimeSpan.FromMinutes(1), _perMinuteCap, now);
                    var wakeAt = nextSecond >= nextMinute ? nextSecond : nextMinute;

                    await ctx.CreateTimer(wakeAt, CancellationToken.None);
                    continue;
                }

                int remaining = list.Count - index;
                int take = Math.Min(available, remaining);

                var batch = new List<Task<TOut>>(take);
                for (int takeIndex = 0; takeIndex < take; takeIndex++)
                {
                    startsInLastMinute.Enqueue(now);
                    var item = list[index + takeIndex];
                    batch.Add(startFunc(ctx, item));
                }

                var batchResults = await Task.WhenAll(batch);
                results.AddRange(batchResults);
                index += take;
            }

            return results;

            static void DequeueOlderThan(Queue<DateTime> startQueue, DateTime threshold)
            {
                while (startQueue.Count > 0 && startQueue.Peek() <= threshold) startQueue.Dequeue();
            }

            static int CountSince(Queue<DateTime> startQueue, DateTime threshold)
            {
                return startQueue.Count(startDateTime => startDateTime >= threshold);
            }

            static DateTime NextFreeTime(Queue<DateTime> startQueue, TimeSpan window, int cap, DateTime now)
            {
                var threshold = now - window;

                var startsInWindow = startQueue
                    .Where(t => t > threshold && t <= now)
                    .ToList();

                if (startsInWindow.Count < cap) return now;

                var freeAt = startsInWindow[0].Add(window);

                return freeAt <= now ? now.AddMilliseconds(1) : freeAt;
            }
        }
    }

}

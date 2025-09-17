using Microsoft.DurableTask;
using NUnit.Framework;
using SFA.DAS.ApprenticeFeedback.Jobs.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Services
{
    public record Input(int Id);
    public record Output(int Id, string Status);

    [TestFixture]
    public class SlidingWindowFanoutServiceTests
    {
        private static Task<Output> DummyStart(
            TaskOrchestrationContext ctx, Input input)
            => Task.FromResult(new Output(input.Id, "OK"));

        [Test]
        public async Task ExecuteAsync_WhenUnderCaps_SendsAll()
        {
            // Arrange
            var start = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(start);
            var items = Enumerable.Range(1, 10).Select(i => new Input(i)).ToList();
            var fanout = new SlidingWindowFanoutService(perSecondCap: 55, perMinuteCap: 3000);

            // Act
            var results = await fanout.ExecuteAsync(
                ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(10));
            Assert.That(results.Select(r => r.Id), Is.EquivalentTo(items.Select(x => x.Id)));
            Assert.That(ctx.Timers, Is.Empty, "Should not need to wait when well below caps.");
        }

        [Test]
        public async Task ExecuteAsync_WhenOverSecondCap_ThenWaitsForNextSecond()
        {
            // Arrange: 60 items => 55 at t0, then 5 at t0+1s
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 60).Select(i => new Input(i)).ToList();
            var fanout = new SlidingWindowFanoutService(perSecondCap: 55, perMinuteCap: 3000);

            // Act
            var results = await fanout.ExecuteAsync(
                ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(60));

            // We should have scheduled exactly one wait to the earliest timestamp + 1s (i.e., t0+1s)
            Assert.That(ctx.Timers.Count, Is.EqualTo(1), "Expected one throttling wait due to 55/sec cap.");
            Assert.That(ctx.Timers[0], Is.EqualTo(t0.AddSeconds(1)));
            // Final "now" should be >= t0+1s
            Assert.That(ctx.CurrentUtcDateTime >= t0.AddSeconds(1), Is.True);
        }

        [Test]
        public async Task ExecuteAsync_WhenUnderSecondCapButOverMinuteCap_WaitsForNextMinute()
        {
            // Arrange: 3005 items -> 3000 at t0, then wait to t0+60s, then 5 more
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 3005).Select(i => new Input(i)).ToList();
            var fanout = new SlidingWindowFanoutService(perSecondCap: 100000, perMinuteCap: 3000); // lift per-second to isolate minute cap

            // Act
            var results = await fanout.ExecuteAsync(
                ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(3005));
            Assert.That(ctx.Timers.Count, Is.EqualTo(1), "Should wait exactly once due to minute cap.");
            Assert.That(ctx.Timers[0], Is.EqualTo(t0.AddMinutes(1)));
        }

        [Test]
        public async Task ExecuteAsync_WhenOverSecondCapAndOverMinuteCapExceeded_ThenWaitsForNextMinute()
        {
            // Arrange: 3200 items
            // At t0: up to min(55, 3000) = 55
            // We’ll need repeated per-second waits and eventually minute-window wait when hitting 3000 in 60s.
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 3200).Select(i => new Input(i)).ToList();
            var fanout = new SlidingWindowFanoutService(perSecondCap: 55, perMinuteCap: 3000);

            // Act
            var results = await fanout.ExecuteAsync(
                ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(3200));

            // We expect many second-level waits (because 55/sec) plus one minute rollover once 3000 reached.
            // The first minute will process at most 3000 items. Remaining 200 after minute window expires.
            // Verify at least one wait to t0+60s exists.
            Assert.That(ctx.Timers.Any(d => d == t0.AddSeconds(60)), Is.True, "Expected a 60s minute-boundary wait due to 3000/min cap.");
        }

        [Test]
        public async Task ExecuteAsync_WhenExactlySecondCap_ThenNoWaitsRequired()
        {
            // Arrange: exactly 55 items
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 55).Select(i => new Input(i)).ToList();
            var fanout = new SlidingWindowFanoutService(perSecondCap: 55, perMinuteCap: 3000);

            // Act
            var results = await fanout.ExecuteAsync(
                ctx, items, DummyStart);

            // Assert: all sent, no waits required because we exactly meet the per-second cap
            Assert.That(results, Has.Count.EqualTo(55));
            Assert.That(ctx.Timers, Is.Empty);
        }

        [Test]
        public void ExecuteAsync_WhenNullArguments_ThrowsArgumentNullExcepton()
        {
            var fanout = new SlidingWindowFanoutService(55, 3000);
            var ctx = new FakeOrchestrationContext(DateTime.UtcNow);
            var items = new[] { new Input(1) };

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                fanout.ExecuteAsync(null!, items, DummyStart));
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                fanout.ExecuteAsync(ctx, (Input[])null, DummyStart));
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                fanout.ExecuteAsync(ctx, items, (Func<TaskOrchestrationContext, Input, Task<Output>> )null));
        }
    }
}
using Microsoft.DurableTask;
using NUnit.Framework;
using SFA.DAS.ApprenticeFeedback.Jobs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Services
{
    public record Input(int Id);
    public record Output(int Id, string Status);

    [TestFixture]
    public class SlidingWindowFanoutServiceTests
    {
        private static Task<Output> DummyStart(TaskOrchestrationContext ctx, Input input)
            => Task.FromResult(new Output(input.Id, "OK"));

        [Test]
        public async Task ExecuteAsync_UnderSecondCap_SendsAll_NoWaits()
        {
            // Arrange
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 10).Select(i => new Input(i)).ToList();
            var fanout = new SlidingWindowFanoutService(perSecondCap: 55);

            // Act
            var results = await fanout.ExecuteAsync(ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(10));
            Assert.That(results.Select(r => r.Id), Is.EquivalentTo(items.Select(x => x.Id)));
            Assert.That(ctx.Timers, Is.Empty, "No timers when well below per-second cap.");
        }

        [Test]
        public async Task ExecuteAsync_OverSecondCap_WaitsExactlyToNextFreeSecond()
        {
            // Arrange: 60 items => 55 at t0, 5 at t0+1s
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 60).Select(i => new Input(i)).ToList();
            var fanout = new SlidingWindowFanoutService(perSecondCap: 55);

            // Act
            var results = await fanout.ExecuteAsync(ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(60));
            Assert.That(ctx.Timers.Count, Is.EqualTo(1), "Expected one throttling wait due to 55/sec cap.");
            Assert.That(ctx.Timers[0], Is.EqualTo(t0.AddSeconds(1)));
            Assert.That(ctx.CurrentUtcDateTime >= t0.AddSeconds(1));
        }

        [Test]
        public async Task ExecuteAsync_ExactlyAtSecondCap_NoWaits()
        {
            // Arrange: exactly 55 items in one second
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 55).Select(i => new Input(i)).ToList();
            var fanout = new SlidingWindowFanoutService(perSecondCap: 55);

            // Act
            var results = await fanout.ExecuteAsync(ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(55));
            Assert.That(ctx.Timers, Is.Empty);
        }

        [Test]
        public async Task ExecuteAsync_LargeBatch_SchedulesRepeatedSecondWaits()
        {
            // Arrange: 170 items with cap 55
            // t0: 55, t0+1s: 55, t0+2s: 55, t0+3s: 5
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 170).Select(i => new Input(i)).ToList();
            var fanout = new SlidingWindowFanoutService(perSecondCap: 55);

            // Act
            var results = await fanout.ExecuteAsync(ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(170));

            // We expect 3 waits: to t0+1s, t0+2s, t0+3s
            Assert.That(ctx.Timers, Has.Count.EqualTo(3));
            Assert.That(ctx.Timers[0], Is.EqualTo(t0.AddSeconds(1)));
            Assert.That(ctx.Timers[1], Is.EqualTo(t0.AddSeconds(2)));
            Assert.That(ctx.Timers[2], Is.EqualTo(t0.AddSeconds(3)));
            Assert.That(ctx.CurrentUtcDateTime, Is.EqualTo(t0.AddSeconds(3)));
        }

        [Test]
        public void ExecuteAsync_NullArguments_Throws()
        {
            var fanout = new SlidingWindowFanoutService(55);
            var ctx = new FakeOrchestrationContext(DateTime.UtcNow);
            var items = new[] { new Input(1) };

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                fanout.ExecuteAsync(null!, items, DummyStart));
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                fanout.ExecuteAsync(ctx, (IEnumerable<Input>)null!, DummyStart));
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                fanout.ExecuteAsync(ctx, items, (Func<TaskOrchestrationContext, Input, Task<Output>>)null!));
        }
    }
}
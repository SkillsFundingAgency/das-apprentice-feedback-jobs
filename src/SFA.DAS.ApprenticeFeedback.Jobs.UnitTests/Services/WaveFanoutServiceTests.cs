using Microsoft.DurableTask;
using NUnit.Framework;
using SFA.DAS.ApprenticeFeedback.Jobs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Services
{
    public record Input(int Id);
    public record Output(int Id, string Status);

    [TestFixture]
    public class WaveFanoutServiceTests
    {
        private const int InterwaveWaitSecs = 5;

        private static Task DelayByAsync(TaskOrchestrationContext ctx, int seconds) =>
            ctx.CreateTimer(ctx.CurrentUtcDateTime.AddSeconds(seconds), CancellationToken.None);

        private static Task<Output> DummyStart(TaskOrchestrationContext ctx, Input input)
            => Task.FromResult(new Output(input.Id, "OK"));

        [Test]
        public async Task ExecuteAsync_UnderCap_SendsAll_InSingleWave_NoWaits()
        {
            // Arrange: 10 items with cap 55 -> one wave, no inter-wave wait
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 10).Select(i => new Input(i)).ToList();
            var sut = new WaveFanoutService(perSecondCap: 55);

            // Act
            var results = await sut.ExecuteAsync(
                ctx, items, DummyStart,
                c => DelayByAsync(c, InterwaveWaitSecs));

            // Assert
            Assert.That(results, Has.Count.EqualTo(10));
            Assert.That(results.Select(r => r.Id), Is.EquivalentTo(items.Select(x => x.Id)));
            Assert.That(ctx.Timers, Is.Empty, "No inter-wave waits when no further items remain.");
        }

        [Test]
        public async Task ExecuteAsync_OverCap_TwoWaves_WaitsBetweenWaves()
        {
            // Arrange: 60 items => wave1: 55 at t0, wait 5s, wave2: 5 at t0+5s
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 60).Select(i => new Input(i)).ToList();
            var sut = new WaveFanoutService(perSecondCap: 55);

            // Act
            var results = await sut.ExecuteAsync(
                ctx, items, DummyStart,
                c => DelayByAsync(c, InterwaveWaitSecs));

            // Assert
            Assert.That(results, Has.Count.EqualTo(60));
            Assert.That(ctx.Timers.Count, Is.EqualTo(1), "Exactly one inter-wave wait expected.");
            Assert.That(ctx.Timers[0], Is.EqualTo(t0.AddSeconds(InterwaveWaitSecs)));
            Assert.That(ctx.CurrentUtcDateTime, Is.EqualTo(t0.AddSeconds(InterwaveWaitSecs)));
        }

        [Test]
        public async Task ExecuteAsync_ExactlyAtCap_SingleWave_NoWaits()
        {
            // Arrange: exactly 55 items -> one wave, no inter-wave wait
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 55).Select(i => new Input(i)).ToList();
            var sut = new WaveFanoutService(perSecondCap: 55);

            // Act
            var results = await sut.ExecuteAsync(
                ctx, items, DummyStart,
                c => DelayByAsync(c, InterwaveWaitSecs));

            // Assert
            Assert.That(results, Has.Count.EqualTo(55));
            Assert.That(ctx.Timers, Is.Empty);
        }

        [Test]
        public async Task ExecuteAsync_WaitsInterwave_AfterSlowestTaskInWave()
        {
            // Arrange: first wave (3 items) completes at 1s, 2s, 3s after t0
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var cap = 3;
            var items = Enumerable.Range(1, 6).Select(i => new Input(i)).ToList();
            var sut = new WaveFanoutService(perSecondCap: cap);

            var due = new Dictionary<int, DateTime>
            {
                [1] = t0.AddSeconds(1),
                [2] = t0.AddSeconds(2),
                [3] = t0.AddSeconds(3)
            };

            Task<Output> SlowUntil(TaskOrchestrationContext c, Input i)
                => due.TryGetValue(i.Id, out var d)
                    ? SlowUntilAsync(c, i, d)
                    : Task.FromResult(new Output(i.Id, "OK"));

            // Act
            var results = await sut.ExecuteAsync(
                ctx, items, SlowUntil,
                c => DelayByAsync(c, InterwaveWaitSecs));

            // Assert
            Assert.That(results, Has.Count.EqualTo(6));

            // Expect timers at 1s, 2s, 3s (from SlowUntil), then 3s + inter-wave wait
            Assert.That(ctx.Timers, Does.Contain(t0.AddSeconds(1)));
            Assert.That(ctx.Timers, Does.Contain(t0.AddSeconds(2)));
            Assert.That(ctx.Timers, Does.Contain(t0.AddSeconds(3)));
            Assert.That(ctx.Timers, Does.Contain(t0.AddSeconds(3 + InterwaveWaitSecs)));

            // Orchestration time should now be at the inter-wave timer
            Assert.That(ctx.CurrentUtcDateTime, Is.EqualTo(t0.AddSeconds(3 + InterwaveWaitSecs)));
        }

        [Test]
        public async Task ExecuteAsync_LargeBatch_SchedulesInterwaveWaitBetweenEachWave()
        {
            // Arrange: 170 items with cap 55 → 4 waves (55,55,55,5) → 3 inter-wave delays
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 170).Select(i => new Input(i)).ToList();
            var sut = new WaveFanoutService(perSecondCap: 55);

            // Act
            var results = await sut.ExecuteAsync(
                ctx, items, DummyStart,
                c => DelayByAsync(c, InterwaveWaitSecs));

            // Assert
            Assert.That(results, Has.Count.EqualTo(170));

            Assert.That(ctx.Timers, Has.Count.EqualTo(3));
            Assert.That(ctx.Timers[0], Is.EqualTo(t0.AddSeconds(InterwaveWaitSecs)));
            Assert.That(ctx.Timers[1], Is.EqualTo(t0.AddSeconds(2 * InterwaveWaitSecs)));
            Assert.That(ctx.Timers[2], Is.EqualTo(t0.AddSeconds(3 * InterwaveWaitSecs)));

            Assert.That(ctx.CurrentUtcDateTime, Is.EqualTo(t0.AddSeconds(3 * InterwaveWaitSecs)));
        }

        [Test]
        public void ExecuteAsync_NullArguments_Throws()
        {
            var ctx = new FakeOrchestrationContext(DateTime.UtcNow);
            var items = new[] { new Input(1) };
            var sut = new WaveFanoutService(perSecondCap: 55);
            Func<TaskOrchestrationContext, Task> delay = c => DelayByAsync(c, InterwaveWaitSecs);

            Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ExecuteAsync(null!, items, DummyStart, delay));
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ExecuteAsync(ctx, (IEnumerable<Input>)null!, DummyStart, delay));
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ExecuteAsync(ctx, items, (Func<TaskOrchestrationContext, Input, Task<Output>>)null!, delay));
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ExecuteAsync(ctx, items, DummyStart, null!));
        }

        private static async Task<Output> SlowUntilAsync(
            TaskOrchestrationContext ctx, Input input, DateTime dueUtc)
        {
            if (dueUtc > ctx.CurrentUtcDateTime)
                await ctx.CreateTimer(dueUtc, CancellationToken.None);

            return new Output(input.Id, "OK");
        }
    }
}

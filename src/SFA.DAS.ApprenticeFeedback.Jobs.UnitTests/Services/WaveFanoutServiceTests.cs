using Castle.Core.Logging;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeFeedback.Jobs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Services
{
    /*public record Input(int Id);
    public record Output(int Id, string Status);

    [TestFixture]
    public class WaveFanoutServiceTests
    {
        private static int InterwaveWaitSecs = 5;
        
        private static Task<Output> DummyStart(TaskOrchestrationContext ctx, Input input)
            => Task.FromResult(new Output(input.Id, "OK"));

        [Test]
        public async Task ExecuteAsync_UnderCap_SendsAll_InSingleWave_NoWaits()
        {
            // Arrange: 10 items with cap 55 -> one wave, no wait afterward
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 10).Select(i => new Input(i)).ToList();
            var sut = new WaveFanoutService(perSecondCap: 55);

            // Act
            var results = await sut.ExecuteAsync(ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(10));
            Assert.That(results.Select(r => r.Id), Is.EquivalentTo(items.Select(x => x.Id)));
            Assert.That(ctx.Timers, Is.Empty, "No inter-wave waits when no further items remain.");
        }

        [Test]
        public async Task ExecuteAsync_OverCap_TwoWaves_WaitsBetweenWaves()
        {
            // Arrange: 60 items => wave1: 55 at t0, then wait InterwaveWaitSecs, wave2: 5 at t0+InterwaveWaitSecs
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 60).Select(i => new Input(i)).ToList();
            var sut = new WaveFanoutService(perSecondCap: 55);

            // Act
            var results = await sut.ExecuteAsync(ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(60));
            Assert.That(ctx.Timers.Count, Is.EqualTo(1), "Exactly one inter-wave wait expected.");
            Assert.That(ctx.Timers[0], Is.EqualTo(t0.AddSeconds(InterwaveWaitSecs)));
            Assert.That(ctx.CurrentUtcDateTime >= t0.AddSeconds(InterwaveWaitSecs));
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
            var results = await sut.ExecuteAsync(ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(55));
            Assert.That(ctx.Timers, Is.Empty);
        }

        [Test]
        public async Task ExecuteAsync_WaitsInterwaveWaitSecs_AfterSlowestTaskInWave()
        {
            // Arrange
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var cap = 3;
            var items = Enumerable.Range(1, 6).Select(i => new Input(i)).ToList();
            var sut = new WaveFanoutService(perSecondCap: cap);

            // absolute due-times for wave 1 (ids 1..3)
            var due = new Dictionary<int, DateTime>
            {
                [1] = t0.AddSeconds(1),
                [2] = t0.AddSeconds(2),
                [3] = t0.AddSeconds(3)
            };
            
            Task<Output> Start(TaskOrchestrationContext c, Input i)
                => due.TryGetValue(i.Id, out var d)
                    ? SlowUntil(c, i, d)
                    : Task.FromResult(new Output(i.Id, "OK"));

            // Act
            var results = await sut.ExecuteAsync(ctx, items, Start);

            // Assert:
            Assert.That(results, Has.Count.EqualTo(6)); // all items returned

            // inter-wave timer is exactly after the slowest (3s) + (1*InterwaveWaitSecs) gap => t0+3+InterwaveWaitSecs
            Assert.That(ctx.Timers, Does.Contain(t0.AddSeconds(3 + InterwaveWaitSecs)));
        }


        [Test]
        public async Task ExecuteAsync_LargeBatch_SchedulesInterwaveWaitSecsBetweenEachWave()
        {
            // Arrange: 170 items with cap 55
            // Waves: t0: 55, t0+InterwaveWaitSecs: 55, t0+(2*InterwaveWaitSecs): 55, t0+(3*InterwaveWaitSecs): 5 (3 inter-wave waits)
            var t0 = new DateTime(2025, 09, 17, 12, 00, 00, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0);
            var items = Enumerable.Range(1, 170).Select(i => new Input(i)).ToList();
            var sut = new WaveFanoutService(perSecondCap: 55);

            // Act
            var results = await sut.ExecuteAsync(ctx, items, DummyStart);

            // Assert
            Assert.That(results, Has.Count.EqualTo(170));

            Assert.That(ctx.Timers, Has.Count.EqualTo(3));
            Assert.That(ctx.Timers[0], Is.EqualTo(t0.AddSeconds(InterwaveWaitSecs)));
            Assert.That(ctx.Timers[1], Is.EqualTo(t0.AddSeconds(2 * InterwaveWaitSecs)));
            Assert.That(ctx.Timers[2], Is.EqualTo(t0.AddSeconds(3 * InterwaveWaitSecs)));

            // Fake context should have advanced to last scheduled time
            Assert.That(ctx.CurrentUtcDateTime, Is.EqualTo(t0.AddSeconds(3 * InterwaveWaitSecs)));
        }

        [Test]
        public void ExecuteAsync_NullArguments_Throws()
        {
            // Arrange
            var ctx = new FakeOrchestrationContext(DateTime.UtcNow);
            var items = new[] { new Input(1) };
            var sut = new WaveFanoutService(perSecondCap: 55);

            // Act + Assert
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ExecuteAsync(null!, items, DummyStart));
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ExecuteAsync(ctx, (IEnumerable<Input>)null!, DummyStart));
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ExecuteAsync(ctx, items, (Func<TaskOrchestrationContext, Input, Task<Output>>)null!));
        }

        private static async Task<Output> SlowUntil(
            TaskOrchestrationContext ctx, Input input, DateTime dueUtc)
        {
            if (dueUtc > ctx.CurrentUtcDateTime)
                await ctx.CreateTimer(dueUtc, CancellationToken.None);

            return new Output(input.Id, "OK");
        }
    }*/
}

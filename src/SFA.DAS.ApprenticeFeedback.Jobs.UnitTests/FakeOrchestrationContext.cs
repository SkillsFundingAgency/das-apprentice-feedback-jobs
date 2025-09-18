using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests
{
    internal sealed class FakeOrchestrationContext : TaskOrchestrationContext
    {
        private DateTime _now;
        private readonly object _input;
        
        public readonly List<DateTime> Timers = new();
        
        public Func<string, object, object> ActivityHandler { get; set; }

        public FakeOrchestrationContext(DateTime startUtc, object? input = null)
        {
            _now = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
            _input = input;
        }

        public override DateTime CurrentUtcDateTime => _now;

        /// <summary>
        /// Advance orchestrator time to the requested fireAt deterministically
        /// </summary>
        /// <param name="fireAt"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task CreateTimer(DateTime fireAt, CancellationToken cancellationToken)
        {
            var target = DateTime.SpecifyKind(fireAt, DateTimeKind.Utc);
            if (target < _now)
                throw new InvalidOperationException("Timer scheduled in the past.");
            
            _now = target;
            Timers.Add(_now);
            return Task.CompletedTask;
        }

        //public override Task<TResult> CallActivityAsync<TResult>(TaskName name, object input = null, TaskOptions options = null)
          //  => throw new NotImplementedException();

        public override Task<TResult> CallActivityAsync<TResult>(
            TaskName name, object input = null, TaskOptions options = null)
        {
            if (ActivityHandler is null)
                throw new NotImplementedException("No ActivityHandler set in test context.");

            var obj = ActivityHandler(name, input);
            return Task.FromResult((TResult)obj!);
        }

        //public override T? GetInput<T>() where T : default
          //  => throw new NotImplementedException();

        public override T GetInput<T>() where T : default
            => (T)_input;

        public override Task<T> WaitForExternalEvent<T>(string eventName, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public override void SendEvent(string instanceId, string eventName, object payload)
            => throw new NotImplementedException();

        public override void SetCustomStatus(object customStatus)
            => throw new NotImplementedException();

        public override Task<TResult> CallSubOrchestratorAsync<TResult>(TaskName orchestratorName, object input = null, TaskOptions options = null)
            => throw new NotImplementedException();

        public override void ContinueAsNew(object newInput = null, bool preserveUnprocessedEvents = true)
            => throw new NotImplementedException();

        public override Guid NewGuid()
            => throw new NotImplementedException();

        public override bool IsReplaying => false;

        public override TaskName Name 
            => throw new NotImplementedException();

        public override string InstanceId 
            => throw new NotImplementedException();

        public override ParentOrchestrationInstance Parent 
            => throw new NotImplementedException();

        protected override ILoggerFactory LoggerFactory 
            => throw new NotImplementedException();
    }
}

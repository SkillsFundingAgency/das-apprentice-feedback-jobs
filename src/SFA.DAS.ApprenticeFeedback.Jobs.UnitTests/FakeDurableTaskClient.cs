using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests
{
    internal sealed class FakeDurableTaskClient : DurableTaskClient
    {
        public FakeDurableTaskClient(string name) 
            : base(name)
        {
        }

        public string? ScheduledName { get; private set; }
        public object? ScheduledInput { get; private set; }
        public string InstanceIdToReturn { get; set; } = "test-instance-1";

        public override ValueTask DisposeAsync()
            => throw new NotImplementedException();

        public override AsyncPageable<OrchestrationMetadata> GetAllInstancesAsync(OrchestrationQuery filter = null)
            => throw new NotImplementedException();

        public override Task<OrchestrationMetadata> GetInstancesAsync(string instanceId, bool getInputsAndOutputs = false, CancellationToken cancellation = default)
            => throw new NotImplementedException();

        public override Task<PurgeResult> PurgeAllInstancesAsync(PurgeInstancesFilter filter, CancellationToken cancellation = default)
            => throw new NotImplementedException();

        public override Task<PurgeResult> PurgeInstanceAsync(string instanceId, CancellationToken cancellation = default)
            => throw new NotImplementedException();

        public override Task RaiseEventAsync(string instanceId, string eventName, object eventPayload = null, CancellationToken cancellation = default)
            => throw new NotImplementedException();

        public override Task ResumeInstanceAsync(string instanceId, string reason = null, CancellationToken cancellation = default)
            => throw new NotImplementedException();

        public override Task<string> ScheduleNewOrchestrationInstanceAsync(
            TaskName orchestratorName,
            object? input = null,
            StartOrchestrationOptions? options = null,
            CancellationToken cancellation = default)
        {
            ScheduledName = orchestratorName;
            ScheduledInput = input;
            return Task.FromResult(InstanceIdToReturn);
        }

        public override Task SuspendInstanceAsync(string instanceId, string reason = null, CancellationToken cancellation = default)
            => throw new NotImplementedException();
        

        public override Task TerminateInstanceAsync(string instanceId, object output = null, CancellationToken cancellation = default)
            => throw new NotImplementedException();

        public override Task<OrchestrationMetadata> WaitForInstanceCompletionAsync(string instanceId, bool getInputsAndOutputs = false, CancellationToken cancellation = default)
            => throw new NotImplementedException();

        public override Task<OrchestrationMetadata> WaitForInstanceStartAsync(string instanceId, bool getInputsAndOutputs = false, CancellationToken cancellation = default)
            => throw new NotImplementedException();
    }
}

using System;
using Baseline.Dates;

namespace Jasper.Messaging.Transports.Configuration
{
    public class RetrySettings
    {
        public TimeSpan Cooldown { get; set; } = 1.Seconds();
        public int FailuresBeforeCircuitBreaks { get; set; } = 3;
        public int MaximumEnvelopeRetryStorage { get; set; } = 100;

        public int RecoveryBatchSize { get; set; } = 100;

        public TimeSpan NodeReassignmentPollingTime { get; set; } = 1.Minutes();
        public TimeSpan FirstNodeReassignmentExecution{ get; set; } = 0.Seconds();

    }

    public class ScheduledJobSettings
    {
        public TimeSpan FirstExecution { get; set; } = 0.Seconds();
        public TimeSpan PollingTime { get; set; } = 5.Seconds();
    }
}

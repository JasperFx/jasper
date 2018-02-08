using System;
using Baseline.Dates;

namespace Jasper.Bus.Transports.Configuration
{
    public class RetrySettings
    {
        public TimeSpan Cooldown { get; set; } = 1.Seconds();
        public int FailuresBeforeCircuitBreaks { get; set; } = 3;
        public int MaximumEnvelopeRetryStorage { get; set; } = 100;

        public int RecoveryBatchSize { get; set; } = 100;



    }
}
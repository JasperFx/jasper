using System;
using Baseline.Dates;

namespace Jasper.Configuration
{
    public class ScheduledJobSettings
    {
        public TimeSpan FirstExecution { get; set; } = 0.Seconds();
        public TimeSpan PollingTime { get; set; } = 5.Seconds();
    }
}

using System;
using Baseline.Dates;

namespace Jasper.Configuration
{
    public class ScheduledJobSettings
    {
        /// <summary>
        /// How long to wait before the first execution of polling
        /// for ready, persisted scheduled messages
        /// </summary>
        public TimeSpan FirstExecution { get; set; } = 0.Seconds();

        /// <summary>
        /// Polling interval for executing scheduled messages
        /// </summary>
        public TimeSpan PollingTime { get; set; } = 5.Seconds();
    }
}

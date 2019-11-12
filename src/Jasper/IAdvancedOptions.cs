using System;
using Jasper.Configuration;
using Newtonsoft.Json;

namespace Jasper
{
    public interface IAdvancedOptions
    {

        /// <summary>
        ///     Default is true. Should Jasper throw an exception on start up if any validation errors
        ///     are detected
        /// </summary>
        bool ThrowOnValidationErrors { get; set; }

        /// <summary>
        /// Timings and sizes around retrying message receiving and sending failures
        /// </summary>
        RetrySettings Retries { get; set; }

        /// <summary>
        ///     Timing configuration around the Scheduled Job feature
        /// </summary>
        ScheduledJobSettings ScheduledJobs { get; set; }

        /// <summary>
        ///     Interval between collecting persisted and queued message metrics
        /// </summary>
        TimeSpan MetricsCollectionSamplingInterval { get; set; }

        /// <summary>
        ///     Used to govern the incoming and outgoing message recovery process by making slowing down
        ///     the recovery process when the local worker queues have this many enqueued
        ///     messages
        /// </summary>
        int MaximumLocalEnqueuedBackPressureThreshold { get; set; }

        /// <summary>
        ///     Polling interval for applying back pressure checking. Default is 2 seconds
        /// </summary>
        TimeSpan BackPressurePollingInterval { get; set; }
    }
}

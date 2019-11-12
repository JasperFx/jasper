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

    }
}

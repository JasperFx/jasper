using System;
using System.Threading;
using Baseline.Dates;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper
{
    public class AdvancedSettings
    {


        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        internal void Cancel()
        {
            _cancellation.Cancel();
        }

        /// <summary>
        /// Default is false. Turn this on to see when every polling DurablityAgent
        /// action executes. Warning, it's a LOT of noise
        /// </summary>
        public bool VerboseDurabilityAgentLogging { get; set; } = false;

        /// <summary>
        /// Duration of time to wait before attempting to "ping" a transport
        /// in an attempt to resume a broken sending circuit
        /// </summary>
        public TimeSpan Cooldown { get; set; } = 1.Seconds();

        /// <summary>
        /// How many times outgoing message sending can fail before tripping
        /// off the circuit breaker functionality. Applies to all transport types
        /// </summary>
        public int FailuresBeforeCircuitBreaks { get; set; } = 3;

        /// <summary>
        /// Caps the number of envelopes held in memory for outgoing retries
        /// if an outgoing transport fails.
        /// </summary>
        public int MaximumEnvelopeRetryStorage { get; set; } = 100;

        /// <summary>
        /// Governs the page size for how many persisted incoming or outgoing messages
        /// will be loaded at one time for attempted retries
        /// </summary>
        public int RecoveryBatchSize { get; set; } = 100;

        /// <summary>
        /// How frequently Jasper will attempt to reassign incoming or outgoing
        /// persisted methods from nodes that are detected to be offline
        /// </summary>
        public TimeSpan NodeReassignmentPollingTime { get; set; } = 1.Minutes();

        /// <summary>
        /// When should the first execution of the node reassignment job
        /// execute after application startup.
        /// </summary>
        public TimeSpan FirstNodeReassignmentExecution { get; set; } = 0.Seconds();

        /// <summary>
        ///     Default is true. Should Jasper throw an exception on start up if any validation errors
        ///     are detected
        /// </summary>
        public bool ThrowOnValidationErrors { get; set; } = true;


        /// <summary>
        ///     Interval between collecting persisted and queued message metrics
        /// </summary>
        public TimeSpan MetricsCollectionSamplingInterval { get; set; } = 5.Seconds();

        /// <summary>
        /// How long to wait before the first execution of polling
        /// for ready, persisted scheduled messages
        /// </summary>
        public TimeSpan ScheduledJobFirstExecution { get; set; } = 0.Seconds();

        /// <summary>
        /// Polling interval for executing scheduled messages
        /// </summary>
        public TimeSpan ScheduledJobPollingTime { get; set; } = 5.Seconds();

        /// <summary>
        /// Newtonsoft.Json serialization settings for messages sent or received
        /// </summary>
        public JsonSerializerSettings JsonSerialization { get; set; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        public int UniqueNodeId { get; } = Guid.NewGuid().ToString().GetDeterministicHashCode();

        public CancellationToken Cancellation => _cancellation.Token;

        /// <summary>
        ///     Get or set the logical Jasper service name. By default, this is
        ///     derived from the name of a custom JasperOptions
        /// </summary>

        public string ServiceName { get; set; }

        /// <summary>
        /// This should probably *only* be used in development or testing
        /// to latch all outgoing message sending
        /// </summary>
        public bool StubAllOutgoingExternalSenders { get; set; }
    }
}

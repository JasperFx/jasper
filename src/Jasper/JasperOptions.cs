using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Baseline;
using Baseline.Dates;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper
{
    /// <summary>
    /// Configures the Jasper messaging transports in your application
    /// </summary>
    public partial class JasperOptions : ITransportsExpression, IAdvancedOptions
    {
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();


        private string _serviceName = "Jasper";

        public JasperOptions()
        {
            ListenForMessagesFrom(TransportConstants.RetryUri);
            ListenForMessagesFrom(TransportConstants.ScheduledUri);
            ListenForMessagesFrom(TransportConstants.RepliesUri);

            ServiceName = "Jasper";

            UniqueNodeId = Guid.NewGuid().ToString().GetDeterministicHashCode();
        }

        [JsonIgnore] public int UniqueNodeId { get; }


        /// <summary>
        ///     Logical service name of this application used for instrumentation purposes
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        [JsonIgnore]
        public string ServiceName
        {
            get => _serviceName;
            set
            {
                if (ServiceName.IsEmpty()) throw new ArgumentNullException(nameof(ServiceName));

                _serviceName = value;
            }
        }

        /// <summary>
        /// Newtonsoft.Json serialization settings for messages received
        /// </summary>
        [JsonIgnore]
        public JsonSerializerSettings JsonSerialization { get; set; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        /// <summary>
        ///     Default is true. Should Jasper throw an exception on start up if any validation errors
        ///     are detected
        /// </summary>
        public bool ThrowOnValidationErrors { get; set; } = true;

        [JsonIgnore] public CancellationToken Cancellation => _cancellation.Token;

        /// <summary>
        /// Timings and sizes around retrying message receiving and sending failures
        /// </summary>
        public RetrySettings Retries { get; set; } = new RetrySettings();


        /// <summary>
        ///     Timing configuration around the Scheduled Job feature
        /// </summary>
        public ScheduledJobSettings ScheduledJobs { get; set; } = new ScheduledJobSettings();


        /// <summary>
        ///     Interval between collecting persisted and queued message metrics
        /// </summary>
        public TimeSpan MetricsCollectionSamplingInterval { get; set; } = 5.Seconds();

        /// <summary>
        ///     Used to govern the incoming and outgoing message recovery process by making slowing down
        ///     the recovery process when the local worker queues have this many enqueued
        ///     messages
        /// </summary>
        public int MaximumLocalEnqueuedBackPressureThreshold { get; set; } = 10000;

        /// <summary>
        ///     Polling interval for applying back pressure checking. Default is 2 seconds
        /// </summary>
        public TimeSpan BackPressurePollingInterval { get; set; } = 2.Seconds();





        internal DurabilityAgent DurabilityAgent { get; set; }


        private readonly IList<ListenerSettings> _listeners = new List<ListenerSettings>();


        public ListenerSettings[] Listeners
        {
            get => _listeners.ToArray();
            set
            {
                _listeners.Clear();
                if (value != null) _listeners.AddRange(value);
            }
        }

        /// <summary>
        ///     Listen for messages at the given uri
        /// </summary>
        /// <param name="uri"></param>
        public IListenerSettings ListenForMessagesFrom(Uri uri)
        {
            var listener = _listeners.FirstOrDefault(x => x.Uri == uri);
            if (listener == null)
            {
                listener = new ListenerSettings
                {
                    Uri = uri
                };

                _listeners.Add(listener);
            }

            return listener;
        }

        /// <summary>
        ///     Establish a message listener to a known location and transport
        /// </summary>
        /// <param name="uriString"></param>
        public IListenerSettings ListenForMessagesFrom(string uriString)
        {
            return ListenForMessagesFrom(uriString.ToUri());
        }

    }
}

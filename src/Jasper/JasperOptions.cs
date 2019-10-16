﻿using System;
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
    public class JasperOptions : ITransportsExpression
    {
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();


        private readonly IList<string> _disabledTransports = new List<string>();


        private readonly IList<Uri> _listeners = new List<Uri>();


        private readonly IList<Subscription> _subscriptions = new List<Subscription>();


        internal readonly IList<Subscription> LocalPublishing = new List<Subscription>();


        private string _machineName;
        private string _serviceName = "Jasper";

        public JasperOptions()
        {
            ListenForMessagesFrom(TransportConstants.RetryUri);
            ListenForMessagesFrom(TransportConstants.ScheduledUri);
            ListenForMessagesFrom(TransportConstants.RepliesUri);

            _machineName = Environment.MachineName;
            ServiceName = "Jasper";

            UniqueNodeId = Guid.NewGuid().ToString().GetDeterministicHashCode();
        }

        /// <summary>
        /// Latch any message publishing to local handlers through the loopback mechanisms. Default is false.
        /// </summary>
        public bool DisableLocalPublishing { get; set; } = false;

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
                NodeId = $"{_serviceName}@{_machineName}";
            }
        }

        /// <summary>
        ///     Environment.MachineName by default. This is used to create the unique node id
        ///     of the running Jasper application that uniquely identifies a running node of this
        ///     application name
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public string MachineName
        {
            get => _machineName;
            set
            {
                if (value.IsEmpty()) throw new ArgumentNullException(nameof(MachineName));

                _machineName = value;
                NodeId = $"{_serviceName}@{_machineName}";
            }
        }

        /// <summary>
        ///     It may be valuable to disable any and all transports when bootstrapping for
        ///     testing local message handling
        /// </summary>
        public bool DisableAllTransports { get; set; }

        /// <summary>
        ///     Newtonsoft.Json serialization settings for messages received
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

        /// <summary>
        ///     The unique id of this instance of the logical Jasper service
        /// </summary>
        [JsonIgnore]
        public string NodeId { get; private set; }


        [JsonIgnore] public CancellationToken Cancellation => _cancellation.Token;

        /// <summary>
        ///     Timings and sizes around retrying message receiving and sending failures
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


        /// <summary>
        ///     Used to control whether or not envelopes being moved to the dead letter queue are permanently stored
        ///     with the related error report
        /// </summary>
        public bool PersistDeadLetterEnvelopes { get; set; } = true;

        public Uri[] Listeners
        {
            get => _listeners.ToArray();
            set
            {
                _listeners.Clear();
                if (value != null) _listeners.AddRange(value);
            }
        }

        /// <summary>
        ///     Array of all subscription rules for publishing messages from this
        ///     application
        /// </summary>
        public Subscription[] Subscriptions
        {
            get => _subscriptions.ToArray();
            set
            {
                _subscriptions.Clear();
                if (value != null) _subscriptions.AddRange(value);
            }
        }

        internal DurabilityAgent DurabilityAgent { get; set; }

        /// <summary>
        ///     Disable an attached transport
        /// </summary>
        /// <param name="protocol"></param>
        public void DisableTransport(string protocol)
        {
            _disabledTransports.Fill(protocol.ToLower());
        }

        /// <summary>
        ///     Enable an attached transport
        /// </summary>
        /// <param name="protocol"></param>
        public void EnableTransport(string protocol)
        {
            _disabledTransports.RemoveAll(x => x == protocol.ToLower());
        }


        /// <summary>
        ///     Listen for messages at the given uri
        /// </summary>
        /// <param name="uri"></param>
        public void ListenForMessagesFrom(Uri uri)
        {
            _listeners.Fill(uri.ToCanonicalUri());
        }

        /// <summary>
        ///     Establish a message listener to a known location and transport
        /// </summary>
        /// <param name="uriString"></param>
        public void ListenForMessagesFrom(string uriString)
        {
            ListenForMessagesFrom(uriString.ToUri());
        }

        /// <summary>
        ///     Find the current state of an attached transport
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public TransportState StateFor(string protocol)
        {
            return _disabledTransports.Contains(protocol.ToLower())
                ? TransportState.Disabled
                : TransportState.Enabled;
        }

        internal void StopAll()
        {
            _cancellation.Cancel();
        }

        /// <summary>
        ///     Add a single subscription
        /// </summary>
        /// <param name="subscription"></param>
        public void AddSubscription(Subscription subscription)
        {
            _subscriptions.Fill(subscription);
        }
    }
}

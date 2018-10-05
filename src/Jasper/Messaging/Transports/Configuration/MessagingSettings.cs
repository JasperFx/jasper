using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Baseline.Reflection;
using Jasper.Conneg;
using Jasper.Http;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Messaging.Transports.Configuration
{
    public class MessagingSettings : ITransportsExpression, IAdvancedOptions
    {

        public MessagingSettings()
        {
            ListenForMessagesFrom(TransportConstants.RetryUri);
            ListenForMessagesFrom(TransportConstants.ScheduledUri);
            ListenForMessagesFrom(TransportConstants.RepliesUri);

            _machineName = Environment.MachineName;
            ServiceName = "Jasper";

            UniqueNodeId = Guid.NewGuid().ToString().GetHashCode();
        }

        /// <summary>
        /// Use carefully! Setting this to "false" will disable the execution
        /// and start up of any registered IHostedService agents
        /// </summary>
        public bool HostedServicesEnabled { get; set; } = true;

        public int UniqueNodeId { get; }


        /// <summary>
        /// Logical service name of this application used for instrumentation purposes
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
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
        /// Environment.MachineName by default. This is used to create the unique node id
        /// of the running Jasper application that uniquely identifies a running node of this
        /// application name
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
        /// It may be valuable to disable any and all transports when bootstrapping for
        /// testing local message handling
        /// </summary>
        public bool DisableAllTransports { get; set; }

        /// <summary>
        /// Newtonsoft.Json serialization settings for messages received
        /// </summary>
        public JsonSerializerSettings JsonSerialization { get; set; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        /// <summary>
        /// Default is true. Should Jasper throw an exception on start up if any validation errors
        /// are detected
        /// </summary>
        public bool ThrowOnValidationErrors { get; set; } = true;

        /// <summary>
        /// The unique id of this instance of the logical Jasper service
        /// </summary>
        public string NodeId { get; private set; }


        // Catches anything from unknown transports
        public readonly IList<Uri> Listeners = new List<Uri>();


        /// <summary>
        /// Listen for messages at the given uri
        /// </summary>
        /// <param name="uri"></param>
        public void ListenForMessagesFrom(Uri uri)
        {
            Listeners.Fill(uri.ToCanonicalUri());
        }

        public readonly IList<SubscriberAddress> KnownSubscribers = new List<SubscriberAddress>();

        private string _machineName;
        private string _serviceName = "Jasper";

        /// <summary>
        /// Add a new outgoing message subscription
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public SubscriberAddress SendTo(Uri uri)
        {
            var subscriber = KnownSubscribers.FirstOrDefault(x => x.Uri == uri) ?? new SubscriberAddress(uri);
            KnownSubscribers.Fill(subscriber);

            return subscriber;
        }

        /// <summary>
        /// Add a new outgoing message subscription
        /// </summary>
        /// <param name="uriString"></param>
        /// <returns></returns>
        public ISubscriberAddress SendTo(string uriString)
        {
            return SendTo(uriString.ToUri());
        }

        /// <summary>
        /// Establish a message listener to a known location and transport
        /// </summary>
        /// <param name="uriString"></param>
        public void ListenForMessagesFrom(string uriString)
        {
            ListenForMessagesFrom(uriString.ToUri());
        }

        /// <summary>
        /// Policies and routing for local message handling
        /// </summary>
        internal WorkersGraph Workers { get; } = new WorkersGraph();


        public async Task ApplyLookups(UriAliasLookup lookups)
        {
            var all = Listeners.Concat(KnownSubscribers.Select(x => x.Uri))
                .Distinct().ToArray();

            await lookups.ReadAliases(all);

            foreach (var subscriberAddress in KnownSubscribers)
            {
                subscriberAddress.ReadAlias(lookups);
            }

            var listeners = Listeners.ToArray();
            Listeners.Clear();

            foreach (var listener in listeners)
            {
                var uri = lookups.Resolve(listener);
                ListenForMessagesFrom(uri);
            }
        }

        private readonly IList<string> _disabledTransports = new List<string>();

        /// <summary>
        /// Find the current state of an attached transport
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public TransportState StateFor(string protocol)
        {
            return _disabledTransports.Contains(protocol.ToLower())
                ? TransportState.Disabled
                : TransportState.Enabled;
        }

        /// <summary>
        /// Disable an attached transport
        /// </summary>
        /// <param name="protocol"></param>
        public void DisableTransport(string protocol)
        {
            _disabledTransports.Fill(protocol.ToLower());
        }

        /// <summary>
        /// Enable an attached transport
        /// </summary>
        /// <param name="protocol"></param>
        public void EnableTransport(string protocol)
        {
            _disabledTransports.RemoveAll(x => x == protocol.ToLower());
        }


        public CancellationToken Cancellation => _cancellation.Token;

        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        internal void StopAll()
        {
            _cancellation.Cancel();
        }

        /// <summary>
        /// Timings and sizes around retrying message receiving and sending failures
        /// </summary>
        public RetrySettings Retries { get; set; } = new RetrySettings();


        /// <summary>
        /// Timing configuration around the Scheduled Job feature
        /// </summary>
        public ScheduledJobSettings ScheduledJobs { get; set; } = new ScheduledJobSettings();


        /// <summary>
        /// Interval between collecting persisted and queued message metrics
        /// </summary>
        public TimeSpan MetricsCollectionSamplingInterval { get; set; } = 5.Seconds();

        /// <summary>
        /// Used to govern the incoming and outgoing message recovery process by making slowing down
        /// the recovery process when the local worker queues have this many enqueued
        /// messages
        /// </summary>
        public int MaximumLocalEnqueuedBackPressureThreshold { get; set; } = 10000;

        /// <summary>
        /// Polling interval for applying back pressure checking. Default is 2 seconds
        /// </summary>
        public TimeSpan BackPressurePollingInterval { get; set; } = 2.Seconds();



        /// <summary>
        /// Used to control whether or not envelopes being moved to the dead letter queue are permanently stored
        /// with the related error report
        /// </summary>
        public bool PersistDeadLetterEnvelopes { get; set; } = true;



        public void ApplyMessageTypeSpecificRules(Envelope envelope)
        {
            if (envelope.Message == null)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Message is required for this operation");
            }

            var messageType = envelope.Message.GetType();
            if (!_messageRules.TryFind(messageType, out var rules))
            {
                rules = findMessageTypeCustomizations(messageType).ToArray();
                _messageRules = _messageRules.AddOrUpdate(messageType, rules);
            }

            foreach (var action in rules)
            {
                action(envelope);
            }
        }

        private IEnumerable<Action<Envelope>> findMessageTypeCustomizations(Type messageType)
        {
            foreach (var att in messageType.GetAllAttributes<ModifyEnvelopeAttribute>())
            {
                yield return e => att.Modify(e);
            }



        }

        private ImHashMap<Type, Action<Envelope>[]> _messageRules = ImHashMap<Type, Action<Envelope>[]>.Empty;


        internal readonly IList<RoutingRule> LocalPublishing = new List<RoutingRule>();
    }


}

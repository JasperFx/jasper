using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Conneg;
using Jasper.Messaging.Configuration;
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

        public int UniqueNodeId { get; }

        // Was ChannelGraph.Name
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

        public Uri DefaultChannelAddress
        {
            get => _defaultChannelAddress;
            set
            {
                if (value != null && value.Scheme == TransportConstants.Loopback)
                {
                    ListenForMessagesFrom(value);
                }

                _defaultChannelAddress = value;
            }
        }

        void ITransportsExpression.DefaultIs(string uriString)
        {
            DefaultChannelAddress = uriString.ToUri();
        }

        void ITransportsExpression.DefaultIs(Uri uri)
        {
            DefaultChannelAddress = uri;
        }

        public void ExecuteAllMessagesLocally()
        {
            DefaultChannelAddress = "loopback://default".ToUri();
        }

        public bool DisableAllTransports { get; set; }

        public JsonSerializerSettings JsonSerialization { get; set; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        public bool ThrowOnValidationErrors { get; set; } = true;

        public MediaSelectionMode MediaSelectionMode { get; set; } = MediaSelectionMode.All;



        public string NodeId { get; private set; }


        // Catches anything from unknown transports
        public readonly IList<Uri> Listeners = new List<Uri>();


        public void ListenForMessagesFrom(Uri uri)
        {
            Listeners.Fill(uri.ToCanonicalUri());
        }

        public readonly IList<SubscriberAddress> KnownSubscribers = new List<SubscriberAddress>();
        private Uri _defaultChannelAddress = TransportConstants.RepliesUri;
        private string _machineName;
        private string _serviceName = "Jasper";

        public SubscriberAddress SendTo(Uri uri)
        {
            var subscriber = KnownSubscribers.FirstOrDefault(x => x.Uri == uri) ?? new SubscriberAddress(uri);
            KnownSubscribers.Fill(subscriber);

            return subscriber;
        }

        public ISubscriberAddress SendTo(string uriString)
        {
            return SendTo(uriString.ToUri());
        }

        public void ListenForMessagesFrom(string uriString)
        {
            ListenForMessagesFrom(uriString.ToUri());
        }

        public WorkersGraph Workers { get; } = new WorkersGraph();


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

        public TransportState StateFor(string protocol)
        {
            return _disabledTransports.Contains(protocol.ToLower())
                ? TransportState.Disabled
                : TransportState.Enabled;
        }

        public void DisableTransport(string protocol)
        {
            _disabledTransports.Fill(protocol.ToLower());
        }

        public void EnableTransport(string protocol)
        {
            _disabledTransports.RemoveAll(x => x == protocol.ToLower());
        }



        public CancellationToken Cancellation => _cancellation.Token;

        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        public void StopAll()
        {
            _cancellation.Cancel();
        }

        public RetrySettings Retries { get; } = new RetrySettings();

        // TODO -- move these underneath Retries and introduce a new value object
        public TimeSpan FirstScheduledJobExecution { get; set; } = 0.Seconds();
        public TimeSpan ScheduledJobPollingTime { get; set; } = 5.Seconds();
        public TimeSpan NodeReassignmentPollingTime { get; set; } = 1.Minutes();
        public TimeSpan FirstNodeReassignmentExecution{ get; set; } = 0.Seconds();


        /// <summary>
        /// Used to govern the incoming and outgoing message recovery process by making slowing down
        /// the recovery process when the local worker queues have this many enqueued
        /// messages
        /// </summary>
        public int MaximumLocalEnqueuedBackPressureThreshold { get; set; } = 2000;

        /// <summary>
        /// Used to control whether or not envelopes being moved to the dead letter queue are permanently stored
        /// with the related error report
        /// </summary>
        public bool PersistDeadLetterEnvelopes { get; set; } = true;

    }
}

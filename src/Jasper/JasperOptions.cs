using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Transports;
using Jasper.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jasper
{
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

            UniqueNodeId = Guid.NewGuid().ToString().GetHashCode();
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

    public class RetrySettings
    {
        public TimeSpan Cooldown { get; set; } = 1.Seconds();
        public int FailuresBeforeCircuitBreaks { get; set; } = 3;
        public int MaximumEnvelopeRetryStorage { get; set; } = 100;

        public int RecoveryBatchSize { get; set; } = 100;

        public TimeSpan NodeReassignmentPollingTime { get; set; } = 1.Minutes();
        public TimeSpan FirstNodeReassignmentExecution { get; set; } = 0.Seconds();
    }

    public class ScheduledJobSettings
    {
        public TimeSpan FirstExecution { get; set; } = 0.Seconds();
        public TimeSpan PollingTime { get; set; } = 5.Seconds();
    }

    public class Subscription
    {
        private string[] _contentTypes = {"application/json"};

        public Subscription()
        {
        }

        public Subscription(Assembly assembly)
        {
            Scope = RoutingScope.Assembly;
            Match = assembly.GetName().Name;
        }

        /// <summary>
        /// How does this rule apply? For all messages? By Namespace? By Assembly?
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public RoutingScope Scope { get; set; } = RoutingScope.All;

        /// <summary>
        /// The outgoing address to send matching messages
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// The legal, accepted content types for the receivers. The default is ["application/json"]
        /// </summary>
        public string[] ContentTypes
        {
            get => _contentTypes;
            set => _contentTypes = value?.Distinct().ToArray() ?? new[] {"application/json"};
        }

        /// <summary>
        /// A type name or namespace name if matching on type or namespace
        /// </summary>
        public string Match { get; set; } = string.Empty;

        /// <summary>
        /// Create a subscription for a specific message type
        /// </summary>
        /// <param name="uri"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Subscription ForType<T>(Uri uri)
        {
            return ForType(typeof(T), uri);
        }

        /// <summary>
        /// Create a subscription for a specific message type
        /// </summary>
        /// <param name="uriString"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Subscription ForType<T>(string uriString)
        {
            return ForType(typeof(T), uriString.ToUri());
        }


        /// <summary>
        /// Create a subscription for a specific message type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Subscription ForType(Type type, Uri uri)
        {
            return new Subscription
            {
                Scope = RoutingScope.Type,
                Match = type.FullName,
                Uri = uri
            };
        }

        /// <summary>
        /// Create a subscription for all messages published in this application
        /// </summary>
        /// <returns></returns>
        public static Subscription All()
        {
            return new Subscription
            {
                Scope = RoutingScope.All
            };
        }

        public bool Matches(Type type)
        {
            switch (Scope)
            {
                case RoutingScope.Assembly:
                    return type.Assembly.GetName().Name.EqualsIgnoreCase(Match);

                case RoutingScope.Namespace:
                    return type.IsInNamespace(Match);

                case RoutingScope.Type:
                    return type.Name.EqualsIgnoreCase(Match) || type.FullName.EqualsIgnoreCase(Match) ||
                           type.ToMessageTypeName().EqualsIgnoreCase(Match);

                case RoutingScope.TypeName:
                    return type.ToMessageTypeName().EqualsIgnoreCase(Match);

                default:
                    return true;
            }
        }


        protected bool Equals(Subscription other)
        {
            return Scope == other.Scope && Equals(Uri, other.Uri) && ContentTypes.SequenceEqual(other.ContentTypes) &&
                   string.Equals(Match, other.Match);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Subscription) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Scope;
                hashCode = (hashCode * 397) ^ (Uri != null ? Uri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ContentTypes != null ? ContentTypes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Match != null ? Match.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Create a subscription for all messages published in this application
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Subscription All(Uri uri)
        {
            var subscription = All();
            subscription.Uri = uri;

            return subscription;
        }

        /// <summary>
        /// Create a subscription for all messages published in this application
        /// </summary>
        /// <param name="uriString"></param>
        /// <returns></returns>
        public static Subscription All(string uriString)
        {
            return All(uriString.ToUri());
        }
    }

    public enum TransportState
    {
        Enabled,
        Disabled
    }

    public interface ITransportsExpression
    {
        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        /// <param name="uri"></param>
        void ListenForMessagesFrom(Uri uri);

        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        void ListenForMessagesFrom(string uriString);

        /// <summary>
        ///     Toggle a transport type to enabled. All transports are enabled by default though
        /// </summary>
        /// <param name="protocol"></param>
        void EnableTransport(string protocol);

        /// <summary>
        ///     Disable a single transport by protocol
        /// </summary>
        /// <param name="protocol"></param>
        void DisableTransport(string protocol);


    }

    public interface IFullTransportsExpression : ITransportsExpression
    {
        /// <summary>
        /// Directs Jasper to set up an incoming message listener for the Uri
        /// specified by IConfiguration[configKey]
        /// </summary>
        /// <param name="configKey">The name of an expected configuration item that holds the designated listener Uri</param>
        void ListenForMessagesFromUriValueInConfig(string configKey);
    }

    public static class TransportsExpressionExtensions
    {
        /// <summary>
        ///     Directs the application to listen at the designated port in a
        ///     fast, but non-durable way
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="port"></param>
        public static void LightweightListenerAt(this ITransportsExpression expression, int port)
        {
            expression.ListenForMessagesFrom($"tcp://localhost:{port}");
        }

        /// <summary>
        ///     Directs the application to listen at the designated port in a
        ///     durable way
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="port"></param>
        public static void DurableListenerAt(this ITransportsExpression expression, int port)
        {
            expression.ListenForMessagesFrom($"tcp://localhost:{port}/durable");
        }
    }
}

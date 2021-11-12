using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Baseline.Dates;
using Baseline.ImTools;
using Jasper.Runtime;
using Jasper.Runtime.Routing;
using Jasper.Serialization;
using Jasper.Transports.Sending;
using Spectre.Console;

#nullable enable

namespace Jasper.Configuration
{
    public enum EndpointMode
    {
        Durable,
        BufferedInMemory,
        Inline
    }

    // TODO -- move this into Oakton
    public interface IDescribesProperties
    {
        IDictionary<string, object> DescribeProperties();
    }

    // TODO -- move this into Oakton itself
    public interface ITreeDescriber
    {
        void Describe(TreeNode parentNode);
    }

    /// <summary>
    ///     Configuration for a single message listener within a Jasper application
    /// </summary>
    public abstract class Endpoint : Subscriber, ICircuitParameters, IDescribesProperties
    {
        private ImHashMap<string, IMessageSerializer?> _serializers = ImHashMap<string, IMessageSerializer?>.Empty;
        private string _name;

        protected Endpoint()
        {
        }

        protected Endpoint(Uri uri)
        {
            Parse(uri);
        }

        internal IMessagingRoot? Root { get; set; }

        internal IMessageSerializer? TryFindSerializer(string contentType)
        {
            if (_serializers.TryFind(contentType, out var serializer))
            {
                return serializer;
            }

            serializer = Root?.Options.Serializers.FirstOrDefault(x => x.ContentType.EqualsIgnoreCase(contentType));
            _serializers = _serializers.AddOrUpdate(contentType, serializer);

            return serializer;
        }

        public void RegisterSerializer(IMessageSerializer serializer)
        {
            _serializers = _serializers.AddOrUpdate(serializer.ContentType, serializer);
        }

        private IMessageSerializer? _defaultSerializer;
        public IMessageSerializer? DefaultSerializer
        {
            get
            {
                return _defaultSerializer ??= TryFindSerializer(EnvelopeConstants.JsonContentType) ?? Root?.Options.Serializers.FirstOrDefault();
            }
            set => _defaultSerializer = value;
        }



        /// <summary>
        ///     Descriptive Name for this listener. Optional.
        /// </summary>
        public string Name
        {
            get => _name ?? Uri?.ToString() ?? "Unknown";
            set => _name = value;
        }

        /// <summary>
        ///     The actual address of the listener, including the transport scheme
        /// </summary>
        public abstract Uri Uri { get; }

        public ExecutionDataflowBlockOptions ExecutionOptions { get; set; } = new ExecutionDataflowBlockOptions();

        public bool IsListener { get; set; }

        public bool IsUsedForReplies { get; set; }


        internal IList<Action<Envelope>> Customizations { get; } = new List<Action<Envelope>>();


        /// <summary>
        ///     Duration of time to wait before attempting to "ping" a transport
        ///     in an attempt to resume a broken sending circuit
        /// </summary>
        public TimeSpan PingIntervalForCircuitResume { get; set; } = 1.Seconds();

        /// <summary>
        ///     How many times outgoing message sending can fail before tripping
        ///     off the circuit breaker functionality. Applies to all transport types
        /// </summary>
        public int FailuresBeforeCircuitBreaks { get; set; } = 3;

        /// <summary>
        ///     Caps the number of envelopes held in memory for outgoing retries
        ///     if an outgoing transport fails.
        /// </summary>
        public int MaximumEnvelopeRetryStorage { get; set; } = 100;


        public ISendingAgent Agent { get; internal set; }

        /// <summary>
        ///     Uri as formulated for replies. Should include a notation
        ///     of "durable" as needed
        /// </summary>
        public abstract Uri ReplyUri();


        public abstract void Parse(Uri uri);


        public abstract void StartListening(IMessagingRoot root, ITransportRuntime runtime);

        protected internal ISendingAgent StartSending(IMessagingRoot root, ITransportRuntime runtime,
            Uri replyUri)
        {
            var sender = root.Settings.StubAllOutgoingExternalSenders ? new NulloSender(Uri) : CreateSender(root);
            return runtime.AddSubscriber(replyUri, sender, this);
        }

        protected abstract ISender CreateSender(IMessagingRoot root);

        internal void Customize(Envelope envelope)
        {
            foreach (var modification in Customizations) modification(envelope);
        }

        public override void AddRoute(MessageTypeRouting routing, IMessagingRoot root)
        {
            if (Agent == null) throw new InvalidOperationException($"The agent has not been initialized for this endpoint");

            routing.AddStaticRoute(Agent);
        }

        public virtual IDictionary<string, object> DescribeProperties()
        {
            var dict = new Dictionary<string, object>
            {
                {nameof(Name), Name},
                {nameof(Mode), Mode},
                {nameof(PingIntervalForCircuitResume), PingIntervalForCircuitResume},
                {nameof(FailuresBeforeCircuitBreaks), PingIntervalForCircuitResume},

            };

            if (Mode == EndpointMode.BufferedInMemory)
            {
                dict.Add(nameof(MaximumEnvelopeRetryStorage), MaximumEnvelopeRetryStorage);

                if (IsListener && Mode != EndpointMode.Inline)
                {
                    dict.Add("ExecutionOptions.MaxDegreeOfParallelism", ExecutionOptions.MaxDegreeOfParallelism);
                    dict.Add("ExecutionOptions.EnsureOrdered", ExecutionOptions.EnsureOrdered);
                    dict.Add("ExecutionOptions.SingleProducerConstrained", ExecutionOptions.SingleProducerConstrained);
                    dict.Add("ExecutionOptions.MaxMessagesPerTask", ExecutionOptions.MaxMessagesPerTask);
                }
            }



            return dict;
        }
    }

    public static class MassTransitHeaders
    {
        /// <summary>
        /// The reason for a message action being taken
        /// </summary>
        public const string Reason = "MT-Reason";

        /// <summary>
        /// The type of exception from a Fault
        /// </summary>
        public const string FaultExceptionType = "MT-Fault-ExceptionType";

        /// <summary>
        /// The exception message from a Fault
        /// </summary>
        public const string FaultMessage = "MT-Fault-Message";

        /// <summary>
        /// The message type from a Fault
        /// </summary>
        public const string FaultMessageType = "MT-Fault-MessageType";

        /// <summary>
        /// The consumer type which faulted
        /// </summary>
        public const string FaultConsumerType = "MT-Fault-ConsumerType";

        /// <summary>
        /// The timestamp when the fault occurred
        /// </summary>
        public const string FaultTimestamp = "MT-Fault-Timestamp";

        /// <summary>
        /// The stack trace from a Fault
        /// </summary>
        public const string FaultStackTrace = "MT-Fault-StackTrace";

        /// <summary>
        /// The number of times the message was retried
        /// </summary>
        public const string FaultRetryCount = "MT-Fault-RetryCount";

        /// <summary>
        /// The endpoint that forwarded the message to the new destination
        /// </summary>
        public const string ForwarderAddress = "MT-Forwarder-Address";

        /// <summary>
        /// The tokenId for the message that was registered with the scheduler
        /// </summary>
        public const string SchedulingTokenId = "MT-Scheduling-TokenId";

        /// <summary>
        /// The number of times the message has been redelivered (zero if never)
        /// </summary>
        public const string RedeliveryCount = "MT-Redelivery-Count";

        /// <summary>
        /// The trigger key that was used when the scheduled message was trigger
        /// </summary>
        public const string QuartzTriggerKey = "MT-Quartz-TriggerKey";

        /// <summary>
        /// Identifies the client from which the request is being sent
        /// </summary>
        public const string ClientId = "MT-Request-ClientId";

        /// <summary>
        /// Identifies the endpoint that handled the request
        /// </summary>
        public const string EndpointId = "MT-Request-EndpointId";

        /// <summary>
        /// The initiating conversation id if a new conversation was started by this message
        /// </summary>
        public const string InitiatingConversationId = "MT-InitiatingConversationId";

        /// <summary>
        /// MessageId - <see cref="MessageEnvelope"/>
        /// </summary>
        public const string MessageId = "MessageId";

        /// <summary>
        /// CorrelationId - <see cref="MessageEnvelope"/>
        /// </summary>
        public const string CorrelationId = "CorrelationId";

        /// <summary>
        /// ConversationId - <see cref="MessageEnvelope"/>
        /// </summary>
        public const string ConversationId = "ConversationId";

        /// <summary>
        /// RequestId - <see cref="MessageEnvelope"/>
        /// </summary>
        public const string RequestId = "RequestId";

        /// <summary>
        /// InitiatorId - <see cref="MessageEnvelope"/>
        /// </summary>
        public const string InitiatorId = "MT-InitiatorId";

        /// <summary>
        /// SourceAddress - <see cref="MessageEnvelope"/>
        /// </summary>
        public const string SourceAddress = "MT-Source-Address";

        /// <summary>
        /// ResponseAddress - <see cref="MessageEnvelope"/>
        /// </summary>
        public const string ResponseAddress = "MT-Response-Address";

        /// <summary>
        /// FaultAddress - <see cref="MessageEnvelope"/>
        /// </summary>
        public const string FaultAddress = "MT-Fault-Address";

        /// <summary>
        /// MessageType - <see cref="MessageEnvelope"/>
        /// </summary>
        public const string MessageType = "MT-MessageType";

        /// <summary>
        /// The Transport message ID, which is a string, because we can't assume anything
        /// </summary>
        public const string TransportMessageId = "TransportMessageId";

        /// <summary>
        /// When a transport header is used, this is the name
        /// </summary>
        public const string ContentType = "Content-Type";


        public static class Host
        {
            public const string Info = "MT-Host-Info";
            public const string MachineName = "MT-Host-MachineName";
            public const string ProcessName = "MT-Host-ProcessName";
            public const string ProcessId = "MT-Host-ProcessId";
            public const string Assembly = "MT-Host-Assembly";
            public const string AssemblyVersion = "MT-Host-AssemblyVersion";
            public const string MassTransitVersion = "MT-Host-MassTransitVersion";
            public const string FrameworkVersion = "MT-Host-FrameworkVersion";
            public const string OperatingSystemVersion = "MT-Host-OperatingSystemVersion";
        }


        public static class Request
        {
            public const string Remaining = "MT-Request-Remaining";

            public const string Accept = "MT-Request-AcceptType";
        }


        public static class Quartz
        {
            /// <summary>
            /// The time when the message was scheduled
            /// </summary>
            public const string Scheduled = "MT-Quartz-Scheduled";

            /// <summary>
            /// When the event for this message was fired by Quartz
            /// </summary>
            public const string Sent = "MT-Quartz-Sent";

            /// <summary>
            /// When the next message is scheduled to be sent
            /// </summary>
            public const string NextScheduled = "MT-Quartz-NextScheduled";

            /// <summary>
            /// When the previous message was sent
            /// </summary>
            public const string PreviousSent = "MT-Quartz-PreviousSent";
        }
    }
}

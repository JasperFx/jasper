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
using Oakton.Descriptions;
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

    /// <summary>
    ///     Configuration for a single message listener within a Jasper application
    /// </summary>
    public abstract class Endpoint : Subscriber, ICircuitParameters, IDescribesProperties
    {
        private ImHashMap<string, IMessageSerializer>? _serializers = ImHashMap<string, IMessageSerializer>.Empty;
        private string _name;

        protected Endpoint()
        {
        }

        protected Endpoint(Uri? uri)
        {
            Parse(uri);
        }

        internal IJasperRuntime? Root { get; set; }

        internal IMessageSerializer? TryFindSerializer(string? contentType)
        {
            if (contentType.IsEmpty()) return null;
            if (_serializers.TryFind(contentType, out var serializer))
            {
                return serializer;
            }

            serializer = Root?.Options.FindSerializer(contentType);
            _serializers = _serializers.AddOrUpdate(contentType, serializer);

            return serializer;
        }

        public void RegisterSerializer(IMessageSerializer? serializer)
        {
            _serializers = _serializers.AddOrUpdate(serializer.ContentType, serializer);
        }

        private IMessageSerializer? _defaultSerializer;
        public IMessageSerializer? DefaultSerializer
        {
            get
            {
                if (_defaultSerializer == null)
                {
                    var parent = Root?.Options.DefaultSerializer;
                    if (parent != null)
                    {
                        // Gives you a chance to use per-endpoint JSON settings for example
                        _defaultSerializer = TryFindSerializer(parent.ContentType);
                    }
                }

                return _defaultSerializer ??= Root?.Options.DefaultSerializer;
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
        public abstract Uri? Uri { get; }

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


        public ISendingAgent? Agent { get; internal set; }

        /// <summary>
        ///     Uri as formulated for replies. Should include a notation
        ///     of "durable" as needed
        /// </summary>
        public abstract Uri? ReplyUri();


        public abstract void Parse(Uri? uri);


        public abstract void StartListening(IJasperRuntime runtime);

        protected internal ISendingAgent StartSending(IJasperRuntime runtime,
            Uri? replyUri)
        {
            var sender = runtime.Advanced.StubAllOutgoingExternalSenders ? new NulloSender(Uri) : CreateSender(runtime);
            return runtime.AddSubscriber(replyUri, sender, this);
        }

        protected abstract ISender CreateSender(IJasperRuntime root);

        internal void Customize(Envelope? envelope)
        {
            foreach (var modification in Customizations) modification(envelope);
        }

        public override void AddRoute(MessageTypeRouting routing, IJasperRuntime runtime)
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
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Baseline.Dates;
using Jasper.Runtime;
using Jasper.Runtime.Routing;
using Jasper.Transports.Sending;

namespace Jasper.Configuration
{
    public enum EndpointMode
    {
        Durable,
        Queued,
        Inline
    }

    /// <summary>
    ///     Configuration for a single message listener within a Jasper application
    /// </summary>
    public abstract class Endpoint : Subscriber, ICircuitParameters
    {
        protected Endpoint()
        {
        }

        protected Endpoint(Uri uri)
        {
            Parse(uri);
        }

        /// <summary>
        ///     Descriptive Name for this listener. Optional.
        /// </summary>
        public string Name { get; set; }

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


        protected internal abstract void StartListening(IMessagingRoot root, ITransportRuntime runtime);

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
    }
}

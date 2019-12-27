using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Jasper.Runtime;
using Jasper.Transports.Sending;

namespace Jasper.Configuration
{
    /// <summary>
    /// Configuration for a single message listener within a Jasper application
    /// </summary>
    public abstract class Endpoint
    {
        protected Endpoint()
        {
        }

        protected Endpoint(Uri uri)
        {
            Parse(uri);
        }

        /// <summary>
        /// Descriptive Name for this listener. Optional.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Uri as formulated for replies. Should include a notation
        /// of "durable" as needed
        /// </summary>
        public abstract Uri ReplyUri();


        public abstract void Parse(Uri uri);

        /// <summary>
        /// The actual address of the listener, including the transport scheme
        /// </summary>
        public abstract Uri Uri { get; }

        /// <summary>
        /// Mark whether or not the receiver for this listener should use
        /// message persistence for durability
        /// </summary>
        public bool IsDurable { get; set; }

        public ExecutionDataflowBlockOptions ExecutionOptions { get; set; } = new ExecutionDataflowBlockOptions();

        public bool IsListener { get; set; }


        protected internal abstract void StartListening(IMessagingRoot root, ITransportRuntime runtime);

        protected internal ISendingAgent StartSending(IMessagingRoot root, ITransportRuntime runtime,
            Uri replyUri)
        {
            var sender = root.Settings.StubAllOutgoingExternalSenders ? new NulloSender(Uri) : CreateSender(root);
            return runtime.AddSubscriber(replyUri, sender, this);
        }

        protected abstract ISender CreateSender(IMessagingRoot root);

        public IList<Subscription> Subscriptions { get; } = new List<Subscription>();
        public bool IsUsedForReplies { get; set; }

    }
}

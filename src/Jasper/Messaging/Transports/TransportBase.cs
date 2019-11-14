using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public abstract class TransportBase : ITransport
    {
        public TransportBase(string protocol)
        {
            Protocol = protocol;
        }



        public string Protocol { get; }
        public Uri ReplyUri { get; protected set; }



        public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
        {
            try
            {
                var batchedSender = createSender(uri, cancellation, root);


                var agent = uri.IsDurable()
                    ? (ISendingAgent)new DurableSendingAgent(uri, batchedSender, root.TransportLogger, root.Settings, root.Persistence)
                    : new LightweightSendingAgent(uri, batchedSender, root.TransportLogger, root.Settings);

                agent.DefaultReplyUri = ReplyUri;
                agent.Start();

                return agent;
            }
            catch (Exception e)
            {
                throw new TransportEndpointException(uri, "Could not build sending agent. See inner exception.", e);
            }
        }

        public void InitializeSendersAndListeners(IMessagingRoot root)
        {
            var options = root.Options;

            var incoming = options.Listeners.Where(x => x.Scheme == Protocol).ToArray();

            incoming = validateAndChooseReplyChannel(incoming);

            foreach (var listenerSettings in incoming)
            {
                buildListener(root, listenerSettings, options);
            }

            // TODO -- later this configuration will be directly on here
            var groups = root.Options.Subscriptions.Where(x => x.Uri.Scheme == Protocol).GroupBy(x => x.Uri);
            foreach (var @group in groups)
            {
                var subscriber = new Subscriber(@group.Key, @group);
                var agent = BuildSendingAgent(subscriber.Uri, root, root.Settings.Cancellation);


                subscriber.StartSending(root.Logger, agent, ReplyUri);

                root.AddSubscriber(subscriber);
            }

        }

        private void buildListener(IMessagingRoot root, ListenerSettings listenerSettings, JasperOptions options)
        {
            try
            {
                var agent = buildListeningAgent(listenerSettings, root.Options.Advanced, root.Handlers, root);

                root.AddListener(listenerSettings, agent);
            }
            catch (Exception e)
            {
                throw new TransportEndpointException(listenerSettings.Uri, "Could not build listening agent. See inner exception.",
                    e);
            }
        }




        public void Dispose()
        {

        }

        protected abstract ISender createSender(Uri uri, CancellationToken cancellation, IMessagingRoot root);

        protected abstract ListenerSettings[] validateAndChooseReplyChannel(ListenerSettings[] incoming);
        protected abstract IListener buildListeningAgent(ListenerSettings listenerSettings,
            AdvancedSettings settings,
            HandlerGraph handlers, IMessagingRoot root);
    }
}

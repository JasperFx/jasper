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
        public TransportBase(string protocol, ITransportLogger logger,
            AdvancedSettings settings)
        {
            this.logger = logger;
            Settings = settings;
            Protocol = protocol;
        }

        public AdvancedSettings Settings { get; }

        protected ITransportLogger logger { get; }

        public string Protocol { get; }
        public Uri ReplyUri { get; protected set; }



        public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
        {
            try
            {
                var batchedSender = createSender(uri, cancellation);


                var agent = uri.IsDurable()
                    ? root.BuildDurableSendingAgent(uri, batchedSender)
                    : new LightweightSendingAgent(uri, batchedSender, logger, Settings);

                agent.DefaultReplyUri = ReplyUri;
                agent.Start();

                return agent;
            }
            catch (Exception e)
            {
                throw new TransportEndpointException(uri, "Could not build sending agent. See inner exception.", e);
            }
        }

        public void StartListening(IMessagingRoot root)
        {
            var options = root.Options;

            var incoming = options.Listeners.Where(x => x.Scheme == Protocol).ToArray();

            incoming = validateAndChooseReplyChannel(incoming);

            foreach (var listenerSettings in incoming)
            {
                buildListener(root, listenerSettings, options);
            }
        }

        private void buildListener(IMessagingRoot root, ListenerSettings listenerSettings, JasperOptions options)
        {
            try
            {
                var agent = buildListeningAgent(listenerSettings, root.Options.Advanced, root.Handlers);

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

        protected abstract ISender createSender(Uri uri, CancellationToken cancellation);

        protected abstract ListenerSettings[] validateAndChooseReplyChannel(ListenerSettings[] incoming);
        protected abstract IListeningAgent buildListeningAgent(ListenerSettings listenerSettings,
            AdvancedSettings settings,
            HandlerGraph handlers);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public abstract class TransportBase : ITransport
    {
        private readonly IDurableMessagingFactory _durableMessagingFactory;
        private readonly IList<IListener> _listeners = new List<IListener>();

        public TransportBase(string protocol, IDurableMessagingFactory factory, ITransportLogger logger, MessagingSettings settings)
        {
            _durableMessagingFactory = factory;
            this.logger = logger;
            MessagingSettings = settings;
            Protocol = protocol;
        }

        public string Protocol { get; }
        public Uri LocalReplyUri { get; protected set; }
        public IWorkerQueue WorkerQueue { get; private set; }

        public MessagingSettings MessagingSettings { get; }

        protected ITransportLogger logger { get; }

        public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
        {
            var batchedSender = createSender(uri, cancellation);

            ISendingAgent agent;

            if (uri.IsDurable())
            {
                agent = _durableMessagingFactory.BuildSendingAgent(uri, batchedSender, cancellation);
            }
            else
            {
                agent = new LightweightSendingAgent(uri, batchedSender, logger, MessagingSettings);
            }

            agent.DefaultReplyUri = LocalReplyUri;
            agent.Start();

            return agent;
        }

        protected abstract ISender createSender(Uri uri, CancellationToken cancellation);

        public void StartListening(IMessagingRoot root)
        {
            var settings = root.Settings;

            if (settings.StateFor(Protocol) == TransportState.Disabled) return;

            WorkerQueue = root.Workers;

            var incoming = settings.Listeners.Where(x => x.Scheme == Protocol).ToArray();

            incoming = validateAndChooseReplyChannel(incoming);

            foreach (var uri in incoming)
            {
                var agent = buildListeningAgent(uri, settings);

                var listener = uri.IsDurable()
                    ? _durableMessagingFactory.BuildListener(agent, root)
                    : new LightweightListener( WorkerQueue, logger, agent);

                _listeners.Add(listener);

                listener.Start();
            }
        }

        protected abstract Uri[] validateAndChooseReplyChannel(Uri[] incoming);
        protected abstract IListeningAgent buildListeningAgent(Uri uri, MessagingSettings settings);

        public void Describe(TextWriter writer)
        {
            foreach (var listener in _listeners)
            {
                writer.WriteLine($"Listening at {listener.Address}");
            }
        }

        public void Dispose()
        {
            foreach (var listener in _listeners)
            {
                listener.SafeDispose();
            }

            _listeners.Clear();
        }
    }
}

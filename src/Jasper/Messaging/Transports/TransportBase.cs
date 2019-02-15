using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
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

        private ListeningStatus _status = ListeningStatus.Accepting;

        public TransportBase(string protocol, IDurableMessagingFactory factory, ITransportLogger logger,
            JasperOptions options)
        {
            _durableMessagingFactory = factory;
            this.logger = logger;
            JasperOptions = options;
            Protocol = protocol;
        }

        public IWorkerQueue WorkerQueue { get; private set; }

        public JasperOptions JasperOptions { get; }

        protected ITransportLogger logger { get; }

        public string Protocol { get; }
        public Uri ReplyUri { get; protected set; }



        public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
        {
            var batchedSender = createSender(uri, cancellation);


            var agent = uri.IsDurable()
                ? _durableMessagingFactory.BuildSendingAgent(uri, batchedSender, cancellation)
                : new LightweightSendingAgent(uri, batchedSender, logger, JasperOptions);

            agent.DefaultReplyUri = ReplyUri;
            agent.Start();

            return agent;
        }

        public void StartListening(IMessagingRoot root)
        {
            var settings = root.Settings;

            if (settings.StateFor(Protocol) == TransportState.Disabled) return;

            WorkerQueue = root.Workers;

            var incoming = settings.Listeners.Where(x => x.Scheme == Protocol).ToArray();

            incoming = validateAndChooseReplyChannel(incoming);

            foreach (var uri in incoming)
            {
                var agent = buildListeningAgent(uri, settings, root.Handlers);
                agent.Status = _status;

                var listener = uri.IsDurable()
                    ? _durableMessagingFactory.BuildListener(agent, root)
                    : new LightweightListener(WorkerQueue, logger, agent);

                _listeners.Add(listener);

                listener.Start();
            }
        }

        public IEnumerable<IListener> Listeners => _listeners;

        public void Describe(TextWriter writer)
        {
            foreach (var listener in _listeners) writer.WriteLine($"Listening at {listener.Address}");
        }

        public ListeningStatus ListeningStatus
        {
            get => _status;
            set
            {
                _status = value;
                foreach (var listener in _listeners) listener.Status = value;
            }
        }

        public void Dispose()
        {
            foreach (var listener in _listeners) listener.SafeDispose();

            _listeners.Clear();
        }

        protected abstract ISender createSender(Uri uri, CancellationToken cancellation);

        protected abstract Uri[] validateAndChooseReplyChannel(Uri[] incoming);
        protected abstract IListeningAgent buildListeningAgent(Uri uri, JasperOptions settings, HandlerGraph handlers);
    }
}

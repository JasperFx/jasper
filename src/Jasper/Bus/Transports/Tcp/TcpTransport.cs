using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Bus.Logging;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Receiving;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.Transports.WorkerQueues;
using Jasper.Util;
using Listener = Jasper.Bus.Transports.Receiving.Listener;

namespace Jasper.Bus.Transports.Tcp
{
    public class TcpTransport : ITransport
    {
        private readonly IPersistence _persistence;
        private readonly CompositeLogger _logger;
        private readonly ILightweightWorkerQueue _lightweightWorkerQueue;
        private readonly IDurableWorkerQueue _durableWorkerQueue;
        private readonly IList<Listener> _listeners = new List<Listener>();
        private Uri _replyUri;

        public TcpTransport(IPersistence persistence, CompositeLogger logger, ILightweightWorkerQueue lightweightWorkerQueue, IDurableWorkerQueue durableWorkerQueue)
        {
            _persistence = persistence;
            _logger = logger;
            _lightweightWorkerQueue = lightweightWorkerQueue;
            _durableWorkerQueue = durableWorkerQueue;

        }


        public string Protocol { get; } = "tcp";


        public ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation)
        {
            var batchedSender = new BatchedSender(uri, new SocketSenderProtocol(), cancellation);

            ISendingAgent agent;

            if (uri.IsDurable())
            {
                agent = new DurableSendingAgent(uri, _persistence, batchedSender, cancellation);
            }
            else
            {
                agent = new LightweightSendingAgent(uri, batchedSender);
            }

            agent.DefaultReplyUri = _replyUri;
            agent.Start();

            return agent;
        }

        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }

        public void StartListening(BusSettings settings)
        {
            if (settings.StateFor(Protocol) == TransportState.Disabled) return;

            var incoming = settings.Listeners.Where(x => x.Scheme == Protocol).ToArray();

            assertNoDuplicatePorts(incoming);

            foreach (var uri in incoming)
            {
                var listener = uri.IsDurable()
                    ? buildDurableListener(uri, settings)
                    : buildLightweightListener(uri, settings);

                _listeners.Add(listener);

                listener.Start();
            }

            if (incoming.Any())
            {
                var port = incoming.First().Port;
                _replyUri = $"{Protocol}://{settings.MachineName}:{port}"
                    .ToUri();
            }
        }

        private Listener buildDurableListener(Uri uri, BusSettings settings)
        {
            var agent = new SocketListeningAgent(uri.Port, settings.Cancellation);
            return new Listener(_persistence, _durableWorkerQueue, _logger, agent);
        }

        private Listener buildLightweightListener(Uri uri, BusSettings settings)
        {
            var agent = new SocketListeningAgent(uri.Port, settings.Cancellation);
            return new Listener(new NulloPersistence(), _lightweightWorkerQueue, _logger, agent);
        }

        // TODO -- throw a more descriptive exception
        private static void assertNoDuplicatePorts(Uri[] incoming)
        {
            var duplicatePorts = incoming.GroupBy(x => x.Port).Where(x => x.Count() > 1).ToArray();
            if (duplicatePorts.Any())
            {
                throw new Exception("You need a better exception here about duplicate ports");
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

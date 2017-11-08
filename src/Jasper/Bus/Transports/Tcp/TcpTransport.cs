using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Bus.Logging;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Receiving;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.WorkerQueues;
using Jasper.Util;

namespace Jasper.Bus.Transports.Tcp
{
    public class TcpTransport : ITransport
    {
        private readonly IPersistence _persistence;
        private readonly CompositeLogger _logger;
        private readonly IWorkerQueue _workerQueue;
        private readonly IList<IListener> _listeners = new List<IListener>();

        public TcpTransport(IPersistence persistence, CompositeLogger logger, IWorkerQueue workerQueue)
        {
            _persistence = persistence;
            _logger = logger;
            _workerQueue = workerQueue;
        }


        public string Protocol { get; } = "tcp";


        public ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation)
        {
            var batchedSender = new BatchedSender(uri, new SocketSenderProtocol(), cancellation);

            ISendingAgent agent;

            if (uri.IsDurable())
            {
                agent = _persistence.BuildSendingAgent(uri, batchedSender, cancellation);
            }
            else
            {
                agent = new LightweightSendingAgent(uri, batchedSender);
            }

            agent.DefaultReplyUri = LocalReplyUri;
            agent.Start();

            return agent;
        }

        public Uri LocalReplyUri { get; private set; }

        public void StartListening(BusSettings settings)
        {
            if (settings.StateFor(Protocol) == TransportState.Disabled) return;

            var incoming = settings.Listeners.Where(x => x.Scheme == Protocol).ToArray();

            assertNoDuplicatePorts(incoming);

            foreach (var uri in incoming)
            {
                var agent = new SocketListeningAgent(uri.Port, settings.Cancellation);
                var listener = uri.IsDurable()
                    ? _persistence.BuildListener(agent, _workerQueue)
                    : new LightweightListener( _workerQueue, _logger, agent);

                _listeners.Add(listener);

                listener.Start();
            }

            if (incoming.Any())
            {
                var uri = incoming.First();
                var port = uri.Port;
                LocalReplyUri = uri.ToMachineUri();
            }
        }

        public void Describe(TextWriter writer)
        {
            foreach (var listener in _listeners)
            {
                writer.WriteLine($"Listening at {listener.Address}");
            }
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

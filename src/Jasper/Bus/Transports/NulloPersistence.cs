using System;
using System.Threading;
using Jasper.Bus.Logging;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Receiving;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.WorkerQueues;

namespace Jasper.Bus.Transports
{
    public class NulloPersistence : IPersistence
    {
        private readonly CompositeLogger _logger;
        private readonly BusSettings _settings;

        public NulloPersistence(CompositeLogger logger, BusSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public ISendingAgent BuildSendingAgent(Uri destination, ISender sender, CancellationToken cancellation)
        {
            return new LightweightSendingAgent(destination, sender, _logger, _settings);
        }

        public ISendingAgent BuildLocalAgent(Uri destination, IWorkerQueue queues)
        {
            return new LoopbackSendingAgent(destination, queues);
        }

        public IListener BuildListener(IListeningAgent agent, IWorkerQueue queues)
        {
            return new LightweightListener(queues, _logger, agent);
        }

        public void ClearAllStoredMessages()
        {
            // nothing
        }
    }
}

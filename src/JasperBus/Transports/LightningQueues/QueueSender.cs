using System;
using System.Collections.Generic;
using JasperBus.Configuration;
using System.Threading.Tasks;
using JasperBus.Runtime;

namespace JasperBus.Transports.LightningQueues
{
    public class QueueSender : ISender
    {
        private readonly Uri _destination;
        private readonly LightningQueue _queue;
        private readonly string _subQueue;

        public QueueSender(Uri destination, LightningQueue queue, string subQueue)
        {
            _destination = destination;
            _queue = queue;
            _subQueue = subQueue;
        }

        public Task Send(Envelope envelope)
        {
            _queue.Send(envelope, _destination, _subQueue);
            return Task.CompletedTask;
        }
    }
}
using System;
using System.Collections.Generic;
using JasperBus.Configuration;

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

        public void Send(byte[] data, IDictionary<string, string> headers)
        {
            _queue.Send(data, headers, _destination, _subQueue);
        }
    }
}
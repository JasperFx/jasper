using System;
using System.Net;

namespace Jasper.Bus.Transports.LightningQueues
{
    public class LightningQueueTransportException : Exception
    {
        public LightningQueueTransportException(IPEndPoint endpoint, Exception innerException) : base("Error trying to initialize LightningQueues queue manager at " + endpoint, innerException)
        {
        }

    }
}
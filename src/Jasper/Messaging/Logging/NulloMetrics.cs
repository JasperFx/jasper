using System;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Logging
{
    public class NulloMetrics : IMetrics
    {
        public void MessageReceived(Envelope envelope)
        {

        }

        public void MessageExecuted(Envelope envelope)
        {
        }

        public void LogException(Exception ex)
        {
        }

        public void CircuitBroken(Uri destination)
        {
        }

        public void CircuitResumed(Uri destination)
        {
        }

        public void LogLocalWorkerQueueDepth(int count)
        {
        }

        public void LogPersistedIncomingMessages(int count)
        {
        }

        public void LogPersistedScheduledMessages(int count)
        {
        }

        public void LogPersistedOutgoingMessages(int count, Uri destination)
        {
        }
    }
}
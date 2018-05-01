using System;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Logging
{
    public interface IMetrics
    {
        void MessageReceived(Envelope envelope);
        void MessageExecuted(Envelope envelope);
        void LogException(Exception ex);

        /// <summary>
        /// The sending agent for this destination experienced too many failures and has been latched
        /// </summary>
        /// <param name="destination"></param>
        void CircuitBroken(Uri destination);

        /// <summary>
        /// The sending agent for this destination has been successfully pinged and un-latched
        /// </summary>
        /// <param name="destination"></param>
        void CircuitResumed(Uri destination);

        void LogLocalWorkerQueueDepth(int count); // delegated from MetricsCollector

        void LogPersistedCounts(PersistedCounts counts);
    }
}

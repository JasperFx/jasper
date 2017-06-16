using System;
using Jasper.Bus.Runtime;

namespace Jasper.Bus
{
    public interface IBusLogger
    {
        void Sent(Envelope envelope);
        void Received(Envelope envelope);
        void ExecutionStarted(Envelope envelope);
        void ExecutionFinished(Envelope envelope);
        void MessageSucceeded(Envelope envelope);
        void MessageFailed(Envelope envelope, Exception ex);

        void LogException(Exception ex, string correlationId = null, string message = "Exception detected:");
        void NoHandlerFor(Envelope envelope);
    }
}
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

    public class NulloBusLogger : IBusLogger
    {
        public void Sent(Envelope envelope)
        {

        }

        public void Received(Envelope envelope)
        {
        }

        public void ExecutionStarted(Envelope envelope)
        {
        }

        public void ExecutionFinished(Envelope envelope)
        {
        }

        public void MessageSucceeded(Envelope envelope)
        {
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
        }

        public void LogException(Exception ex, string correlationId = null, string message = "Exception detected:")
        {
        }

        public void NoHandlerFor(Envelope envelope)
        {
        }
    }
}

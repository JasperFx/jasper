using System;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Logging
{
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
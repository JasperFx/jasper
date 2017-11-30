using System;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Diagnostics.Messages;
using Jasper.WebSockets;

namespace Jasper.Diagnostics
{
    // TODO -- turn this into a subclass of BusLoggerBase after Preston's diagnostics changes get in
    public class DiagnosticsBusLogger : IBusLogger
    {
        private readonly Lazy<IWebSocketSender> _client;

        public DiagnosticsBusLogger(Lazy<IWebSocketSender> client)
        {
            _client = client;
        }

        public void ExecutionFinished(Envelope envelope)
        {
        }

        public void ExecutionStarted(Envelope envelope)
        {
        }

        public void LogException(Exception ex, string correlationId = null, string message = "Exception detected:")
        {
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
            _client.Value.Send(new MessageFailed(envelope, ex));
        }

        public void MessageSucceeded(Envelope envelope)
        {
            _client.Value.Send(new MessageSucceeded(envelope));
        }

        public void NoHandlerFor(Envelope envelope)
        {
        }

        public void NoRoutesFor(Envelope envelope)
        {

        }

        public void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {

        }

        public void MovedToErrorQueue(Envelope envelope, Exception ex)
        {

        }

        public void Received(Envelope envelope)
        {
        }

        public void Sent(Envelope envelope)
        {
            _client.Value.Send(new MessageSent(envelope));
        }
    }
}

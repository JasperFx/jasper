using System;
using Jasper.Remotes.Messaging;
using JasperBus;
using JasperBus.Runtime;

namespace Jasper.Diagnostics
{
    public class DiagnosticsBusLogger : IBusLogger
    {
        private readonly IDiagnosticsClient _client;

        public DiagnosticsBusLogger(IDiagnosticsClient client)
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
            _client.Send(new MessageFailed());
        }

        public void MessageSucceeded(Envelope envelope)
        {
            _client.Send(new MessageSucceeded());
        }

        public void NoHandlerFor(Envelope envelope)
        {
        }

        public void Received(Envelope envelope)
        {
        }

        public void Sent(Envelope envelope)
        {
        }
    }

    public class MessageSucceeded : ClientMessage
    {
        public MessageSucceeded() : base("bus-message-succeeded")
        {
        }
    }

    public class MessageFailed : ClientMessage
    {
        public MessageFailed() : base("bus-message-failed")
        {
        }
    }
}

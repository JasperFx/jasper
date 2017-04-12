using System;
using Jasper.Diagnostics.Messages;
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
            _client.Send(new MessageFailed(envelope, ex));
        }

        public void MessageSucceeded(Envelope envelope)
        {
            _client.Send(new MessageSucceeded(envelope));
        }

        public void NoHandlerFor(Envelope envelope)
        {
        }

        public void Received(Envelope envelope)
        {
        }

        public void Sent(Envelope envelope)
        {
            _client.Send(new MessageSent(envelope));
        }
    }
}

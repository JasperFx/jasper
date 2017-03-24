using System;
using System.Collections.Generic;
using Jasper.Diagnostics.Messages;
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
        }
    }

    public class MessageSucceeded : ClientMessage
    {
        public MessageSucceeded(Envelope envelope) : base("bus-message-succeeded")
        {
            Envelope = new EnvelopeModel(envelope);
        }

        public EnvelopeModel Envelope { get; }
    }

    public class MessageFailed : ClientMessage
    {
        public MessageFailed(Envelope envelope, Exception exception) : base("bus-message-failed")
        {
            Envelope = new EnvelopeModel(envelope, exception);
        }

        public EnvelopeModel Envelope { get; }
    }

    public class EnvelopeModel
    {
        public EnvelopeModel(Envelope envelope, Exception exception = null)
        {
            Headers = envelope.Headers;
            CorrelationId = envelope.CorrelationId;
            ParentId = envelope.ParentId;
            Description = envelope.ToString();
            Source = envelope.Source;
            Destination = envelope.Destination;
            Message = envelope.Message;
            MessageType = new MessageTypeModel(envelope.Message?.GetType());
            Attempts = envelope.Attempts;
            Exception = exception?.Message;
            StackTrace = exception?.ToString();
            HasError = exception != null ? true : false;
        }

        public IDictionary<string, string> Headers { get; }

        public string CorrelationId { get; }
        public string ParentId { get; }
        public MessageTypeModel MessageType { get; }
        public string Description { get; }
        public Uri Source { get; }
        public Uri Destination { get; }
        public object Message { get; }
        public int Attempts { get; }
        public bool HasError { get; }
        public string Exception { get; set; }
        public string StackTrace { get; set;}
    }
}

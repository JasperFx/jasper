using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Transports.Lightweight
{
    public class LightweightCallback : IMessageCallback
    {
        private readonly IChannel _retries;

        public LightweightCallback(IChannel retries)
        {
            _retries = retries ?? throw new ArgumentNullException(nameof(retries));
        }

        public Task MarkSuccessful()
        {
            return Task.CompletedTask;
        }

        public Task MarkFailed(Exception ex)
        {
            throw new InlineMessageException("Failed while invoking an inline message", ex);
        }

        public Task MoveToErrors(ErrorReport report)
        {
            // TODO -- need a general way to log errors against an ITransport
            return Task.CompletedTask;
        }

        public Task Send(Envelope envelope)
        {
            // nothing
            return Task.CompletedTask;
        }

        public bool SupportsSend { get; } = false;
        public string TransportScheme => "retries";

        public Task Requeue(Envelope envelope)
        {
            var clone = envelope.Clone();
            clone.Destination = _retries.Destination;

            return _retries.Send(clone);
        }
    }
}

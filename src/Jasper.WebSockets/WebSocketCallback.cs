using System;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;

namespace Jasper.WebSockets
{
    public class WebSocketCallback : IMessageCallback
    {
        private readonly IChannel _retries;

        public WebSocketCallback(IChannel retries)
        {
            _retries = retries;
        }

        public Task MarkSuccessful()
        {
            return Task.CompletedTask;
        }

        public Task MarkFailed(Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task MoveToErrors(ErrorReport report)
        {
            return Task.CompletedTask;
        }

        public Task Requeue(Envelope envelope)
        {
            return _retries.Send(envelope);
        }

        public Task Send(Envelope envelope)
        {
            throw new NotSupportedException();
        }

        public bool SupportsSend { get; } = false;
        public string TransportScheme { get; } = "ws";
    }
}
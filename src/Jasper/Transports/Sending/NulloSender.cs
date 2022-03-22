using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jasper.Transports.Sending
{
    public class NulloSender : ISender
    {
        public NulloSender(Uri? destination)
        {
            Destination = destination;
        }

        public void Dispose()
        {
        }

        public bool SupportsNativeScheduledSend { get; } = false;
        public Uri? Destination { get; }
        public Task Send(Envelope? envelope) => Task.CompletedTask;
        public Task<bool> Ping(CancellationToken cancellationToken) => Task.FromResult(true);
    }
}

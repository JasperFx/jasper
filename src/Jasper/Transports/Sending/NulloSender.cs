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
        public Uri Destination { get; }
        public ValueTask Send(Envelope envelope) => new ValueTask();
        public Task<bool> Ping(CancellationToken cancellationToken) => Task.FromResult(true);
    }
}

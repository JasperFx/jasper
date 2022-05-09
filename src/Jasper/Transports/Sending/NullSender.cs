using System;
using System.Threading.Tasks;

namespace Jasper.Transports.Sending;

public class NullSender : ISender
{
    public NullSender(Uri destination)
    {
        Destination = destination;
    }

    public void Dispose()
    {
    }

    public bool SupportsNativeScheduledSend => false;
    public Uri Destination { get; }

    public ValueTask SendAsync(Envelope envelope)
    {
        return ValueTask.CompletedTask;
    }

    public Task<bool> PingAsync()
    {
        return Task.FromResult(true);
    }
}

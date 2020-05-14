using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jasper.Transports.Sending
{
    public interface ISenderRequiresCallback : IDisposable
    {
        void RegisterCallback(ISenderCallback senderCallback);
    }

    public interface ISender : IDisposable
    {
        bool SupportsNativeScheduledSend { get; }
        Uri Destination { get; }
        Task<bool> Ping(CancellationToken cancellationToken);
        Task Send(Envelope envelope);
    }
}

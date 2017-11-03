using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.Sending
{
    public interface ISender : IDisposable
    {
        void Start(ISenderCallback callback);

        Task Enqueue(Envelope envelope);
        Uri Destination { get; }

        int QueuedCount { get; }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jasper.Transports.Sending
{
    public interface ISender : IDisposable
    {
        Uri Destination { get; }

        int QueuedCount { get; }

        bool Latched { get; }
        void Start(ISenderCallback callback);

        Task Enqueue(Envelope envelope);

        Task LatchAndDrain();
        void Unlatch();

        /// <summary>
        ///     Simply try to reach the endpoint to verify it can receive
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Ping(CancellationToken cancellationToken);

        bool SupportsNativeScheduledSend { get; }


    }
}

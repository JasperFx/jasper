using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Transports.Sending
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
        /// <returns></returns>
        Task Ping();
    }
}

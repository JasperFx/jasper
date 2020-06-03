using System;
using Jasper.Runtime;

namespace Jasper.Transports
{
    public interface IListener : IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
        void Start(IListeningWorkerQueue callback);

        /// <summary>
        /// This starts a listener in the "inline" native transport mode
        /// such that the incoming messages are processed inline rather than
        /// being queued locally
        /// </summary>
        /// <param name="pipeline"></param>
        void StartHandlingInline(IHandlerPipeline pipeline);
    }
}

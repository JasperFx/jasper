using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Receiving;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.WorkerQueues;

namespace Jasper.Bus.Transports
{
    public interface  IPersistence
    {
        ISendingAgent BuildSendingAgent(Uri destination, ISender sender, CancellationToken cancellation);
        ISendingAgent BuildLocalAgent(Uri destination, IWorkerQueue queues);
        IListener BuildListener(IListeningAgent agent, IWorkerQueue queues);
        void ClearAllStoredMessages();

        Task ScheduleJob(Envelope envelope);

        Task<ErrorReport> LoadDeadLetterEnvelope(Guid id);
    }
}

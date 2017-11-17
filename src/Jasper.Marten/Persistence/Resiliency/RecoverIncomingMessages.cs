using System;
using System.Threading.Tasks;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.WorkerQueues;
using Marten;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class RecoverIncomingMessages : IMessagingAction
    {
        private readonly IWorkerQueue _workers;

        public RecoverIncomingMessages(IWorkerQueue workers, BusSettings settings, StoreOptions storeOptions)
        {
            _workers = workers;


        }

        public Task Execute(IDocumentSession session)
        {
            // try to get the "jasper-incoming" lock
            // if so, pull 100 messages. Assign all to this node
            // put in worker queues

            throw new NotImplementedException();
        }
    }
}

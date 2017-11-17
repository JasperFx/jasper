using System;
using System.Threading.Tasks;
using Marten;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class RecoverIncomingMessages : IMessagingAction
    {
        public Task Execute(IDocumentSession session)
        {
            // try to get the "jasper-incoming" lock
            // if so, pull 100 messages. Assign all to this node
            // put in worker queues

            throw new NotImplementedException();
        }
    }
}
using System;
using System.Threading.Tasks;
using Marten;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class RecoverOutgoingMessages : IMessagingAction
    {
        public Task Execute(IDocumentSession session)
        {
            // try to get the "jasper-outgoing" lock
            // if so, pull 100 messages. Assign all to this node
            // put in sender agents
            // will need an IChannel.QuickSend()

            throw new NotImplementedException();
        }
    }
}
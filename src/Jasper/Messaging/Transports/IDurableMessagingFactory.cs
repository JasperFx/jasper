using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports
{
    public interface  IDurableMessagingFactory
    {
        ISendingAgent BuildSendingAgent(Uri destination, ISender sender, CancellationToken cancellation);
        ISendingAgent BuildLocalAgent(Uri destination, IMessagingRoot root);
        IListener BuildListener(IListeningAgent agent, IMessagingRoot root);
        void ClearAllStoredMessages();

        Task ScheduleJob(Envelope envelope);


    }
}

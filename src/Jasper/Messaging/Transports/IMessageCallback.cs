using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Transports
{
    public interface IMessageCallback
    {
        Task MarkComplete();

        Task MoveToErrors(Envelope envelope, Exception exception);


        Task Requeue(Envelope envelope);

        Task MoveToScheduledUntil(DateTimeOffset time, Envelope envelope);
    }
}

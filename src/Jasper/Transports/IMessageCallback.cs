using System;
using System.Threading.Tasks;

namespace Jasper.Transports
{
    public interface IMessageCallback
    {
        Task MarkComplete();

        Task MoveToErrors(Envelope envelope, Exception exception);


        Task Requeue(Envelope envelope);

        Task MoveToScheduledUntil(DateTimeOffset time, Envelope envelope);
    }
}

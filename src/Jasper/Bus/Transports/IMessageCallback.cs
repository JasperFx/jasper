using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports
{
    public interface IMessageCallback
    {
        Task MarkComplete();

        Task MoveToErrors(Envelope envelope, Exception exception);


        Task Requeue(Envelope envelope);

        Task MoveToDelayedUntil(DateTimeOffset time, Envelope envelope);
    }
}

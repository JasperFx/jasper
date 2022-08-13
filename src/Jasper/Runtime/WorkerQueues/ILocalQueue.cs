using Jasper.Transports;

namespace Jasper.Runtime.WorkerQueues;

public interface ILocalQueue : IReceiver
{
    void Enqueue(Envelope envelope);
}

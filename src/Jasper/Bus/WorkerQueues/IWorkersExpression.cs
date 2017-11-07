using System.ComponentModel;

namespace Jasper.Bus.WorkerQueues
{
    public interface IWorkersExpression
    {
        IWorkerSettings Worker(string queueName);
    }
}

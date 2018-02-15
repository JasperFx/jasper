namespace Jasper.Messaging.WorkerQueues
{
    public interface IWorkersExpression
    {
        IWorkerSettings Worker(string queueName);
    }
}

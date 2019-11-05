namespace Jasper.Messaging.WorkerQueues
{
    public interface IWorkersExpression
    {
        IListenerSettings Worker(string queueName);
    }
}

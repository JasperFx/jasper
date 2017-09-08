namespace Jasper.Bus.Settings
{
    public interface IQueueIndexer
    {
        QueueSettings this[string queueName] { get; }

        bool Has(string queueName);
    }
}
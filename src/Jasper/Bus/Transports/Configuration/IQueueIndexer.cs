namespace Jasper.Bus.Transports.Configuration
{
    public interface IQueueIndexer
    {
        QueueSettings this[string queueName] { get; }

        bool Has(string queueName);
    }
}
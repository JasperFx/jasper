namespace Jasper.Bus.Transports.Configuration
{
    public interface ILoopbackTransportExpression
    {
        IQueueSettings Queue(string queueName);
        IQueueSettings DefaultQueue { get; }
    }
}
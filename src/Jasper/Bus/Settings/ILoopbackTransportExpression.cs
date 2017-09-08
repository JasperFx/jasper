namespace Jasper.Bus.Settings
{
    public interface ILoopbackTransportExpression
    {
        IQueueSettings Queue(string queueName);
        IQueueSettings DefaultQueue { get; }
    }
}
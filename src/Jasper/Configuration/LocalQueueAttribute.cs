using System;

namespace Jasper.Configuration
{
    /// <summary>
    /// Directs Jasper to send this to the named local queue when
    /// using IMessageContext.Enqueue(message)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class LocalQueueAttribute : Attribute
    {
        public string QueueName { get; }

        public LocalQueueAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}

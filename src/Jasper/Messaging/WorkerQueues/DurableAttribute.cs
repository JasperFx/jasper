using System;

namespace Jasper.Messaging.WorkerQueues
{
    /// <summary>
    /// Marks a Jasper message type as being durable on calls to IServiceBus.Enqueue()
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class DurableAttribute : Attribute
    {

    }
}
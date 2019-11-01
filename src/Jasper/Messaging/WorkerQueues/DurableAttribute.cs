using System;

namespace Jasper.Messaging.WorkerQueues
{
    /// <summary>
    ///     Marks a Jasper message type as being durable on calls to IServiceBus.Enqueue()
    /// </summary>
    [Obsolete("Eliminate with GH-557")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class DurableAttribute : Attribute
    {
    }
}

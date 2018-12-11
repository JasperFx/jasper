using System;

namespace Jasper.Messaging.WorkerQueues
{
    public interface IMessagesExpression
    {
        IMessagesExpression Message<T>();
        IMessagesExpression Messages(Func<Type, bool> filter);
    }
}

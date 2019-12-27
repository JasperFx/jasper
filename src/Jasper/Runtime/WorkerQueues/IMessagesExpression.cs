using System;

namespace Jasper.Runtime.WorkerQueues
{
    public interface IMessagesExpression
    {
        IMessagesExpression Message<T>();
        IMessagesExpression Messages(Func<Type, bool> filter);
    }
}

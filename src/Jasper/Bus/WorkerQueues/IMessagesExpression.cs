using System;

namespace Jasper.Bus.WorkerQueues
{
    public interface IMessagesExpression
    {
        IMessagesExpression Message<T>();
        IMessagesExpression Messages(Func<Type, bool> filter);
    }
}
using System;

namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public interface IPublishing
    {
        
        IPublishing Message<T>();
        IPublishing Message(Type messageType);
        
        /// <summary>
        /// Mark concrete types in the application assembly matching this filter
        /// as published events
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        IPublishing MessagesMatching(Func<Type, bool> filter);
    }
}
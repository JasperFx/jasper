using System;

namespace Jasper.Bus.Runtime.Routing
{
    public interface IMessageRouter
    {
        void ClearAll();
        MessageRoute[] Route(Type messageType);
    }
}
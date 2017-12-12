using System;
using System.Threading.Tasks;

namespace Jasper.Bus.Runtime.Routing
{
    public interface IMessageRouter
    {
        void ClearAll();
        Task<MessageRoute[]> Route(Type messageType);
        Task<MessageRoute> RouteForDestination(Envelope envelopeDestination);

        Task<Envelope[]> Route(Envelope envelope);
    }
}

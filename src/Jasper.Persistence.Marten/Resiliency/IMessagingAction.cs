using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Marten;

namespace Jasper.Persistence.Marten.Resiliency
{
    public interface IMessagingAction
    {
        Task Execute(IDocumentSession session, ISchedulingAgent agent);
    }
}

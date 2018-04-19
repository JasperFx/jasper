using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Marten;

namespace Jasper.Marten.Persistence.Resiliency
{
    public interface IMessagingAction
    {
        Task Execute(IDocumentSession session, ISchedulingAgent agent);
    }
}

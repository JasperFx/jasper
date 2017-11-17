using System.Threading.Tasks;
using Marten;

namespace Jasper.Marten.Persistence.Resiliency
{
    public interface IMessagingAction
    {
        Task Execute(IDocumentSession session);
    }
}
using System.Threading.Tasks;

namespace Jasper.Persistence.Durability
{
    public interface IMessagingAction
    {
        string Description { get; }
        Task Execute(IEnvelopePersistence storage, IDurabilityAgent agent);
    }
}

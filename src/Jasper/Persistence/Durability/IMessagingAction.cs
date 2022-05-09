using System.Threading.Tasks;

namespace Jasper.Persistence.Durability;

public interface IMessagingAction
{
    string Description { get; }
    Task ExecuteAsync(IEnvelopePersistence storage, IDurabilityAgent agent);
}

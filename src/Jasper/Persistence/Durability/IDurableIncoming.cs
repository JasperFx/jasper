using System.Threading.Tasks;

namespace Jasper.Persistence.Durability
{
    public interface IDurableIncoming
    {
        Task<Envelope[]> LoadPageOfLocallyOwnedIncoming();
        Task ReassignIncoming(int ownerId, Envelope[] incoming);
    }
}

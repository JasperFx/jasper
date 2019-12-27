using System.Threading.Tasks;

namespace Jasper.Persistence.Durability
{
    public interface IDurableIncoming
    {
        Task<Envelope[]> LoadPageOfLocallyOwned();
        Task Reassign(int ownerId, Envelope[] incoming);
    }
}

using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Durability
{
    public interface IDurableIncoming
    {
        Task<Envelope[]> LoadPageOfLocallyOwned();
        Task Reassign(int ownerId, Envelope[] incoming);
    }
}
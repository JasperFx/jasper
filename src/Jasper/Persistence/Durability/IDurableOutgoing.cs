using System;
using System.Threading.Tasks;

namespace Jasper.Persistence.Durability
{
    public interface IDurableOutgoing
    {
        Task<Envelope[]> Load(Uri destination);
        Task Reassign(int ownerId, Envelope[] outgoing);
        Task DeleteByDestination(Uri destination);
        Task Delete(Envelope[] outgoing);
        Task<Uri[]> FindAllDestinations();


    }
}

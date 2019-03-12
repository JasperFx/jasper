using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Durability
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
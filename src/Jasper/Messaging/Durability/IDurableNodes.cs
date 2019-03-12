using System.Threading.Tasks;

namespace Jasper.Messaging.Durability
{
    public interface IDurableNodes
    {
        Task ReassignDormantNodeToAnyNode(int nodeId);
        Task<int[]> FindUniqueOwners(int currentNodeId);
    }
}
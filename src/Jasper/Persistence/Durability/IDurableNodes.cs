using System.Threading.Tasks;

namespace Jasper.Persistence.Durability
{
    public interface IDurableNodes
    {
        Task ReassignDormantNodeToAnyNode(int nodeId);
        Task<int[]> FindUniqueOwners(int currentNodeId);
    }
}

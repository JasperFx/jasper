using System;
using System.Threading.Tasks;
using Jasper.Logging;

namespace Jasper.Persistence.Durability
{
    public interface IDurableStorageSession : IDisposable
    {
        Task ReleaseNodeLock(int lockId);
        Task GetNodeLock(int lockId);
        Task Begin();
        Task Commit();
        Task Rollback();

        Task<bool> TryGetGlobalTxLock(int lockId);
        Task<bool> TryGetGlobalLock(int lockId);
        Task ReleaseGlobalLock(int lockId);

        bool IsConnected();
        Task ConnectAndLockCurrentNode(ITransportLogger logger, int nodeId);
    }
}

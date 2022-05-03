using System;
using System.Data.Common;
using System.Threading.Tasks;
using Jasper.Logging;
using Microsoft.Extensions.Logging;

namespace Jasper.Persistence.Durability
{
    public interface IDurableStorageSession : IDisposable
    {
        Task ReleaseNodeLockAsync(int lockId);
        Task GetNodeLockAsync(int lockId);
        Task BeginAsync();
        Task CommitAsync();
        Task RollbackAsync();

        Task<bool> TryGetGlobalTxLock(int lockId);
        Task<bool> TryGetGlobalLock(int lockId);
        Task ReleaseGlobalLock(int lockId);

        bool IsConnected();
        Task ConnectAndLockCurrentNodeAsync(ILogger logger, int nodeId);
        DbTransaction Transaction { get; }
        DbCommand CallFunction(string functionName);
        DbCommand CreateCommand(string sql);
    }
}

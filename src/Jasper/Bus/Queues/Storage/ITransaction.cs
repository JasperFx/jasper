using System;

namespace Jasper.Bus.Queues.Storage
{
    public interface ITransaction
    {
        Guid TransactionId { get; }
        void Commit();
        void Rollback();
    }
}

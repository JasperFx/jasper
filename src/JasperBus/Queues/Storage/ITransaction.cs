using System;

namespace JasperBus.Queues.Storage
{
    public interface ITransaction
    {
        Guid TransactionId { get; }
        void Commit();
        void Rollback();
    }
}

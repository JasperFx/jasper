using System;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Queues.Storage
{
    public interface IMessageStore : IDisposable
    {
        ITransaction BeginTransaction();
        void CreateQueue(string queueName);
        void StoreIncomingMessages(params Envelope[] messages);
        void StoreIncomingMessages(ITransaction transaction, params Envelope[] messages);
        void DeleteIncomingMessages(params Envelope[] messages);
        IObservable<Envelope> PersistedMessages(string queueName);
        IObservable<OutgoingMessage> PersistedOutgoingMessages();
        void MoveToQueue(ITransaction transaction, string queueName, Envelope message);
        void SuccessfullyReceived(ITransaction transaction, Envelope message);
        void StoreOutgoing(ITransaction tx, OutgoingMessage message);
        int FailedToSend(OutgoingMessage message);
        void SuccessfullySent(params OutgoingMessage[] messages);
        Envelope GetMessage(string queueName, MessageId messageId);
        string[] GetAllQueues();
        void ClearAllStorage();
    }
}

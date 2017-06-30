using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Queues.Storage
{
    public class NoStorage : IMessageStore
    {
        private readonly List<string> _queues = new List<string>();

        private class NoStorageTransaction : ITransaction
        {
            public Guid TransactionId => Guid.Empty;

            public void Commit()
            {
            }

            public void Rollback()
            {
            }
        }

        public void Dispose()
        {
        }

        public ITransaction BeginTransaction()
        {
            return new NoStorageTransaction();
        }

        public void CreateQueue(string queueName)
        {
            _queues.Add(queueName);
        }

        public void StoreIncomingMessages(params Envelope[] messages)
        {
        }

        public void StoreIncomingMessages(ITransaction transaction, params Envelope[] messages)
        {
        }

        public void DeleteIncomingMessages(params Envelope[] messages)
        {
        }

        public IObservable<Envelope> PersistedMessages(string queueName)
        {
            return Observable.Empty<Envelope>();
        }

        public IObservable<OutgoingMessage> PersistedOutgoingMessages()
        {
            return Observable.Empty<OutgoingMessage>();
        }

        public void MoveToQueue(ITransaction transaction, string queueName, Envelope message)
        {
            message.Queue = queueName;
        }

        public void SuccessfullyReceived(ITransaction transaction, Envelope message)
        {
        }

        public void StoreOutgoing(ITransaction tx, OutgoingMessage message)
        {
        }

        public void StoreOutgoing(ITransaction tx, OutgoingMessage[] message)
        {
        }

        public int FailedToSend(OutgoingMessage message)
        {
            return message.SentAttempts;
        }

        public void SuccessfullySent(params OutgoingMessage[] messages)
        {
        }

        public Envelope GetMessage(string queueName, MessageId messageId)
        {
            return null;
        }

        public string[] GetAllQueues()
        {
            return _queues.ToArray();
        }

        public void ClearAllStorage()
        {
        }
    }
}

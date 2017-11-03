using System;
using System.Collections.Generic;
using System.Threading;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports
{
    public interface IPersistence
    {
        void Remove(string queueName, IEnumerable<Envelope> envelopes);
        void Remove(string queueName, Envelope envelope);
        void Replace(string queueName, Envelope envelope);
        void StoreInitial(Envelope[] messages);
        void Remove(Envelope[] messages);
        void RemoveOutgoing(IList<Envelope> outgoingMessages);
        void PersistBasedOnSentAttempts(OutgoingMessageBatch batch, int maxAttempts);
        void Initialize(string[] queueNames);
        void StoreOutgoing(Envelope envelope);
        void ClearAllStoredMessages(string queuePath = null);

        void RecoverOutgoingMessages(Action<Envelope> action, CancellationToken cancellation);
        void RecoverPersistedMessages(string[] queueNames, Action<Envelope> action, CancellationToken cancellation);
    }

    public class NulloPersistence : IPersistence
    {
        public void Remove(string queueName, IEnumerable<Envelope> envelopes)
        {
        }

        public void Remove(string queueName, Envelope envelope)
        {
        }

        public void Replace(string queueName, Envelope envelope)
        {
        }

        public void ReadAll(string name, Action<Envelope> callback)
        {
        }

        public void StoreInitial(Envelope[] messages)
        {
        }

        public void Remove(Envelope[] messages)
        {
        }

        public void RemoveOutgoing(IList<Envelope> outgoingMessages)
        {
        }

        public void PersistBasedOnSentAttempts(OutgoingMessageBatch batch, int maxAttempts)
        {
        }

        public void Initialize(string[] queueNames)
        {
        }

        public void ReadOutgoing(Action<Envelope> callback)
        {
        }

        public void StoreOutgoing(Envelope envelope)
        {
        }

        public void ClearAllStoredMessages(string queuePath = null)
        {

        }

        public void RecoverOutgoingMessages(Action<Envelope> action, CancellationToken cancellation)
        {

        }

        public void RecoverPersistedMessages(string[] queueNames, Action<Envelope> action, CancellationToken cancellation)
        {

        }
    }
}

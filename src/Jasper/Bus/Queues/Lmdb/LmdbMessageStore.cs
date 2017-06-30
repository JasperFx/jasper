using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Jasper.Bus.Queues.Serialization;
using Jasper.Bus.Queues.Storage;
using Jasper.Bus.Runtime;
using LightningDB;

namespace Jasper.Bus.Queues.Lmdb
{
    public class LmdbMessageStore : IMessageStore
    {
        private readonly LightningEnvironment _environment;

        private const string OutgoingQueue = "outgoing";

        public LmdbMessageStore(string path, EnvironmentConfiguration config)
        {
            _environment = new LightningEnvironment(path, config);
            _environment.Open(EnvironmentOpenFlags.WriteMap | EnvironmentOpenFlags.NoSync);
            CreateQueue(OutgoingQueue);
        }

        public LmdbMessageStore(string path) : this(path, new EnvironmentConfiguration {MapSize = 1024 * 1024 * 100, MaxDatabases = 5})
        {
        }

        public LmdbMessageStore(LightningEnvironment environment)
        {
            _environment = environment;
            CreateQueue(OutgoingQueue);
        }

        public LightningEnvironment Environment => _environment;

        public void StoreIncomingMessages(params Envelope[] messages)
        {
            using (var tx = _environment.BeginTransaction())
            {
                StoreIncomingMessages(tx, messages);
                tx.Commit();
            }
        }

        public void StoreIncomingMessages(ITransaction transaction, params Envelope[] messages)
        {
            var tx = ((LmdbTransaction) transaction).Transaction;
            StoreIncomingMessages(tx, messages);
        }

        private void StoreIncomingMessages(LightningTransaction tx, params Envelope[] messages)
        {
            try
            {
                foreach (var messagesByQueue in messages.GroupBy(x => x.Queue))
                {
                    var db = OpenDatabase(tx, messagesByQueue.Key);
                    foreach (var message in messagesByQueue)
                    {
                        tx.Put(db, message.Id.MessageIdentifier.ToByteArray(), message.Serialize());
                    }
                }
            }
            catch (LightningException ex)
            {
                if (ex.StatusCode == LightningDB.Native.Lmdb.MDB_NOTFOUND)
                    throw new QueueDoesNotExistException("Queue doesn't exist", ex);
                throw;
            }
        }

        public void DeleteIncomingMessages(params Envelope[] messages)
        {
            using (var tx = _environment.BeginTransaction())
            {
                foreach (var grouping in messages.GroupBy(x => x.Queue))
                {
                    RemoveMessageFromStorage(tx, grouping.Key, grouping.ToArray());
                }
                tx.Commit();
            }
        }

        public ITransaction BeginTransaction()
        {
            return new LmdbTransaction(_environment);
        }

        public int FailedToSend(Envelope message)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var result = FailedToSend(tx, message);
                tx.Commit();
                return result;
            }
        }

        public void SuccessfullySent(params Envelope[] messages)
        {
            using (var tx = _environment.BeginTransaction())
            {
                SuccessfullySent(tx, messages);
                tx.Commit();
            }
        }

        public Envelope GetMessage(string queueName, MessageId messageId)
        {
            using (var tx = _environment.BeginTransaction(TransactionBeginFlags.ReadOnly))
            {
                var db = OpenDatabase(tx, queueName);
                return tx.Get(db, messageId.MessageIdentifier.ToByteArray()).ToEnvelope();
            }
        }

        public string[] GetAllQueues()
        {
            return GetAllQueuesImpl().Where(x => OutgoingQueue != x).ToArray();
        }

        public void ClearAllStorage()
        {
            var databases = GetAllQueuesImpl().ToArray();
            using (var tx = _environment.BeginTransaction())
            {
                foreach (var databaseName in databases)
                {
                    var db = OpenDatabase(tx, databaseName);
                    tx.TruncateDatabase(db);
                }
                tx.Commit();
            }
        }

        private IEnumerable<string> GetAllQueuesImpl()
        {
            using (var tx = _environment.BeginTransaction(TransactionBeginFlags.ReadOnly))
            using (var db = tx.OpenDatabase())
            using (var cursor = tx.CreateCursor(db))
            {
                foreach (var item in cursor)
                {
                    yield return Encoding.UTF8.GetString(item.Key);
                }
            }
        }

        private void SuccessfullySent(LightningTransaction tx, params Envelope[] messages)
        {
            RemoveMessageFromStorage(tx, OutgoingQueue, messages);
        }

        public IObservable<Envelope> PersistedMessages(string queueName)
        {
            return Observable.Create<Envelope>(x =>
            {
                try
                {
                    using (var tx = _environment.BeginTransaction(TransactionBeginFlags.ReadOnly))
                    {
                        var db = OpenDatabase(tx, queueName);
                        using (var cursor = tx.CreateCursor(db))
                            while (cursor.MoveNext())
                            {
                                var current = cursor.Current;
                                var message = current.Value.ToEnvelope();
                                x.OnNext(message);
                            }
                    }
                    x.OnCompleted();
                }
                catch (Exception ex)
                {
                    x.OnError(ex);
                }
                return Disposable.Empty;
            });
        }

        public IObservable<Envelope> PersistedOutgoingMessages()
        {
            return Observable.Create<Envelope>(x =>
            {
                try
                {
                    using (var tx = _environment.BeginTransaction(TransactionBeginFlags.ReadOnly))
                    {
                        var db = OpenDatabase(tx, OutgoingQueue);
                        using (var cursor = tx.CreateCursor(db))
                            while (cursor.MoveNext())
                            {
                                var current = cursor.Current;
                                var msg = current.Value.ToOutgoingMessage();
                                x.OnNext(msg);
                            }
                    }
                    x.OnCompleted();
                }
                catch (Exception ex)
                {
                    x.OnError(ex);
                }
                return Disposable.Empty;
            });
        }

        public void MoveToQueue(ITransaction transaction, string queueName, Envelope message)
        {
            var tx = ((LmdbTransaction) transaction).Transaction;
            MoveToQueue(tx, queueName, message);
        }

        public void SuccessfullyReceived(ITransaction transaction, Envelope message)
        {
            var tx = ((LmdbTransaction) transaction).Transaction;
            SuccessfullyReceived(tx, message);
        }

        private void SuccessfullyReceived(LightningTransaction tx, Envelope message)
        {
            RemoveMessageFromStorage(tx, message.Queue, message);
        }

        private void RemoveMessageFromStorage(LightningTransaction tx, string queueName, params Envelope[] messages)
        {
            var db = OpenDatabase(tx, queueName);
            foreach (var message in messages)
            {
                tx.Delete(db, message.Id.MessageIdentifier.ToByteArray());
            }
        }

        public void StoreOutgoing(ITransaction transaction, Envelope message)
        {
            var tx = ((LmdbTransaction) transaction).Transaction;
            StoreOutgoing(tx, message);
        }

        public void StoreOutgoing(ITransaction transaction, Envelope[] messages)
        {
            var tx = ((LmdbTransaction) transaction).Transaction;
            foreach (var message in messages)
            {
                StoreOutgoing(tx, message);
            }
        }

        private void StoreOutgoing(LightningTransaction tx, Envelope message)
        {
            var db = OpenDatabase(tx, OutgoingQueue);
            tx.Put(db, message.Id.MessageIdentifier.ToByteArray(), message.SerializeOutgoing());
        }

        private int FailedToSend(LightningTransaction tx, Envelope message)
        {
            var db = OpenDatabase(tx, OutgoingQueue);
            var value = tx.Get(db, message.Id.MessageIdentifier.ToByteArray());
            if (value == null)
                return int.MaxValue;
            var msg = value.ToOutgoingMessage();
            int attempts = message.SentAttempts;
            if (attempts >= message.MaxAttempts)
            {
                RemoveMessageFromStorage(tx, OutgoingQueue, msg);
            }
            else if (msg.DeliverBy.HasValue)
            {
                var expire = msg.DeliverBy.Value;
                if (expire != DateTime.MinValue && DateTime.Now >= expire)
                {
                    RemoveMessageFromStorage(tx, OutgoingQueue, msg);
                }
            }
            else
            {
                tx.Put(db, message.Id.MessageIdentifier.ToByteArray(), message.SerializeOutgoing());
            }
            return attempts;
        }

        private void MoveToQueue(LightningTransaction tx, string queueName, Envelope message)
        {
            try
            {
                var idBytes = message.Id.MessageIdentifier.ToByteArray();
                var original = OpenDatabase(tx, message.Queue);
                var newDb = OpenDatabase(tx, queueName);
                tx.Delete(original, idBytes);
                tx.Put(newDb, idBytes, message.Serialize());
            }
            catch (LightningException ex)
            {
                tx.Dispose();
                if (ex.StatusCode == LightningDB.Native.Lmdb.MDB_NOTFOUND)
                    throw new QueueDoesNotExistException("Queue doesn't exist", ex);
                throw;
            }
        }

        public void CreateQueue(string queueName)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = tx.OpenDatabase(queueName, new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
                _databaseCache[queueName] = db;
                tx.Commit();
            }
        }

        private readonly ConcurrentDictionary<string, LightningDatabase> _databaseCache = new ConcurrentDictionary<string, LightningDatabase>();
        private LightningDatabase OpenDatabase(LightningTransaction transaction, string database)
        {
            if (_databaseCache.ContainsKey(database))
                return _databaseCache[database];
            var db = transaction.OpenDatabase(database);
            _databaseCache[database] = db;
            return db;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~LmdbMessageStore()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var database in _databaseCache)
                {
                    database.Value.Dispose();
                }
                GC.SuppressFinalize(this);
            }
            _environment.Dispose();
        }
    }
}

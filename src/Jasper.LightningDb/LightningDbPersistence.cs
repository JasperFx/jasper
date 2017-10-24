using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.Durable;
using Jasper.Bus.Transports.Util;
using LightningDB;

namespace Jasper.LightningDb
{
    public class LightningDbPersistence : IPersistence, IDisposable
    {
        // TODO -- need to validate db names actually exist

        public const string Outgoing = "outgoing";
        public const string Delayed = "delayed";

        private readonly LightningEnvironment _environment;

        private readonly ConcurrentDictionary<string, LightningDatabase> _databaseCache = new ConcurrentDictionary<string, LightningDatabase>();

        public LightningDbPersistence(LightningDbSettings settings)
        {
            _environment = settings.ToEnvironment();
            _environment.Open(EnvironmentOpenFlags.WriteMap | EnvironmentOpenFlags.NoSync);

            OpenDatabase(Outgoing);
            OpenDatabase(Delayed);
        }

        public void OpenDatabases(string[] names)
        {
            foreach (var name in names)
            {
                OpenDatabase(name);
            }
        }

        public void OpenDatabase(string name)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = tx.OpenDatabase(name, new DatabaseConfiguration {Flags = DatabaseOpenFlags.Create});
                _databaseCache[name] = db;
                tx.Commit();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~LightningDbPersistence()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var database in _databaseCache.Values)
                {
                    database.Dispose();
                }

                GC.SuppressFinalize(this);
            }

            _environment.Dispose();
        }

        public void ClearAllStorage()
        {
            try
            {
                var databases = _databaseCache.Values.ToArray();
                using (var tx = _environment.BeginTransaction())
                {
                    foreach (var db in databases)
                    {
                        tx.TruncateDatabase(db);
                    }

                    tx.Commit();
                }
            }
            catch (Exception)
            {
                // only used in automated testing anyway
            }
        }

        public void Store(string queueName, IEnumerable<Envelope> envelopes)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[queueName];

                foreach (var persistable in envelopes)
                {
                    tx.Put(db, persistable.Identity(), persistable.Serialize());
                }

                tx.Commit();
            }
        }

        public void Store(string queueName, Envelope envelope)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[queueName];

                tx.Put(db, envelope.Identity(), envelope.Serialize());

                tx.Commit();
            }
        }

        public void Remove(string queueName, IEnumerable<Envelope> envelopes)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[queueName];

                foreach (var persistable in envelopes)
                {
                    tx.Delete(db, persistable.Identity());
                }

                tx.Commit();
            }
        }

        public void Remove(string queueName, Envelope envelope)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[queueName];

                tx.Delete(db, envelope.Identity());

                tx.Commit();
            }
        }

        public void Replace(string queueName, Envelope envelope)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[queueName];

                tx.Delete(db, envelope.Identity());
                envelope.EnvelopeVersionId = PersistedMessageId.GenerateRandom();

                tx.Put(db, envelope.Identity(), envelope.Serialize());

                tx.Commit();
            }
        }

        public void Move(string from, string to, Envelope envelope)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var fromDb = _databaseCache[from];
                var toDb = _databaseCache[to];

                tx.Delete(fromDb, envelope.Identity());
                tx.Put(toDb, envelope.Identity(), envelope.Serialize());

                tx.Commit();
            }
        }

        public Envelope Load(string name, PersistedMessageId id)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[name];

                // TODO -- get this inside of PersistedMessageId itself

                var bytes = tx.Get(db, id.MessageIdentifier.ToByteArray());
                return Envelope.Read(bytes);
            }
        }

        public void ReadAll(string name, Action<Envelope> callback, CancellationToken cancellation)
        {
            using (var tx = _environment.BeginTransaction(TransactionBeginFlags.ReadOnly))
            {
                var db = _databaseCache[name];
                using (var cursor = tx.CreateCursor(db))
                    while (cursor.MoveNext() && !cancellation.IsCancellationRequested)
                    {
                        var current = cursor.Current;
                        var envelope = Envelope.Read(current.Value);
                        callback(envelope);
                    }
            }
        }

        public void RecoverOutgoingMessages(Action<Envelope> action, CancellationToken cancellation)
        {
            Task.Factory.StartNew(() =>
            {
                ReadAll(Outgoing, action, cancellation);
            }, cancellation);
        }

        public void RecoverPersistedMessages(string[] queueNames, Action<Envelope> action, CancellationToken cancellation)
        {
            foreach (var queueName in queueNames)
            {
                Task.Factory.StartNew(() =>
                {
                    ReadAll(queueName, action, cancellation);
                }, cancellation);
            }
        }

        public void StoreOutgoing(Envelope envelope)
        {
            Store(Outgoing, envelope);
        }

        public void ClearAllStoredMessages(string queuePath = null)
        {
            var fileSystem = new FileSystem();
            if (queuePath == null)
            {
                //Find all queues matching queuePath regardless of port.
                var jasperQueuePath = new LightningDbSettings().QueuePath;
                queuePath = fileSystem.GetDirectory(jasperQueuePath);

                var queues = fileSystem
                    .ChildDirectoriesFor(queuePath)
                    .Where(x => x.StartsWith(jasperQueuePath, StringComparison.OrdinalIgnoreCase));

                queues.Each(x => fileSystem.DeleteDirectory(x));
            }
            else
            {
                fileSystem.DeleteDirectory(queuePath);
            }
        }

        public IList<Envelope> LoadAll(string name)
        {
            var list = new List<Envelope>();

            ReadAll(name, e => list.Add(e), CancellationToken.None);

            return list;
        }


        public void PersistBasedOnSentAttempts(OutgoingMessageBatch batch, int maxAttempts)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[Outgoing];
                foreach (var envelope in batch.Messages.Where(x => x.SentAttempts >= maxAttempts))
                {
                    tx.Delete(db, envelope.Identity());
                }

                foreach (var envelope in batch.Messages.Where(x => x.SentAttempts < maxAttempts))
                {
                    tx.Delete(db, envelope.Identity());
                    envelope.EnvelopeVersionId = PersistedMessageId.GenerateRandom();

                    tx.Put(db, envelope.Identity(), envelope.Serialize());
                }

                tx.Commit();
            }
        }

        public void StoreInitial(Envelope[] messages)
        {
            using (var tx = _environment.BeginTransaction())
            {
                foreach (var envelope in messages)
                {
                    var db = _databaseCache[envelope.Queue];
                    tx.Put(db, envelope.Identity(), envelope.Serialize());
                }

                tx.Commit();
            }
        }

        public void Remove(Envelope[] messages)
        {
            using (var tx = _environment.BeginTransaction())
            {
                foreach (var envelope in messages)
                {
                    var db = _databaseCache[envelope.Queue];
                    tx.Delete(db, envelope.Identity());
                }

                tx.Commit();
            }
        }

        public void RemoveOutgoing(IList<Envelope> outgoingMessages)
        {
            Remove(Outgoing, outgoingMessages);
        }

        public void Initialize(string[] queueNames)
        {
            foreach (var name in queueNames)
            {
                OpenDatabase(name);
            }
        }
    }

}

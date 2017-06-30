using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Queues;
using Jasper.Bus.Queues.Serialization;
using Jasper.Bus.Runtime;
using LightningDB;

namespace Jasper.LightningDb
{
    public class LocalPersistence : IDisposable
    {
        public const string Outgoing = "outgoing";
        public const string Delayed = "delayed";

        private readonly LightningDbSettings _settings;
        private readonly LightningEnvironment _environment;

        private readonly ConcurrentDictionary<string, LightningDatabase> _databaseCache = new ConcurrentDictionary<string, LightningDatabase>();

        public LocalPersistence(LightningDbSettings settings)
        {
            _settings = settings;
            _environment = settings.ToEnvironment();
            _environment.Open(EnvironmentOpenFlags.WriteMap | EnvironmentOpenFlags.NoSync);

            openDatabase(Outgoing);
            openDatabase(Delayed);
        }

        public void OpenDatabases(string[] names)
        {
            foreach (var name in names)
            {
                openDatabase(name);
            }
        }

        private void openDatabase(string name)
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

        ~LocalPersistence()
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

        public void ClearAllStorage()
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

        public void Store(string databaseName, IEnumerable<Envelope> envelopes)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[databaseName];

                foreach (var persistable in envelopes)
                {
                    tx.Put(db, persistable.Identity(), persistable.Serialize());
                }

                tx.Commit();
            }
        }

        public void Store(string databaseName, Envelope envelope)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[databaseName];

                tx.Put(db, envelope.Identity(), envelope.Serialize());

                tx.Commit();
            }
        }

        public void Remove(string databaseName, IEnumerable<Envelope> envelopes)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[databaseName];

                foreach (var persistable in envelopes)
                {
                    tx.Delete(db, persistable.Identity());
                }

                tx.Commit();
            }
        }

        public void Remove(string databaseName, Envelope envelope)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[databaseName];

                tx.Delete(db, envelope.Identity());

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

        public Envelope Load(string name, MessageId id)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[name];

                // TODO -- get this inside of MessageId itself

                return tx.Get(db, id.MessageIdentifier.ToByteArray()).ToEnvelope();
            }
        }

        public void ReadAll(string name, Action<Envelope> callback)
        {
            using (var tx = _environment.BeginTransaction(TransactionBeginFlags.ReadOnly))
            {
                var db = _databaseCache[name];
                using (var cursor = tx.CreateCursor(db))
                    while (cursor.MoveNext())
                    {
                        var current = cursor.Current;
                        var envelope = current.Value.ToEnvelope();
                        callback(envelope);
                    }
            }
        }

        public IList<Envelope> LoadAll(string name)
        {
            var list = new List<Envelope>();

            ReadAll(name, e => list.Add(e));

            return list;
        }
    }

}

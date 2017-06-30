using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        public void Store(string databaseName, IEnumerable<IPersistable> persistables)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[databaseName];

                foreach (var persistable in persistables)
                {
                    tx.Put(db, persistable.Identity(), persistable.Body());
                }
            }
        }

        public void Remove(string databaseName, IEnumerable<IPersistable> persistables)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var db = _databaseCache[databaseName];

                foreach (var persistable in persistables)
                {
                    tx.Delete(db, persistable.Identity());
                }
            }
        }

        public void Move(string from, string to, IPersistable persistable)
        {
            using (var tx = _environment.BeginTransaction())
            {
                var fromDb = _databaseCache[from];
                var toDb = _databaseCache[to];

                tx.Delete(fromDb, persistable.Identity());
                tx.Put(toDb, persistable.Identity(), persistable.Body());
            }
        }
    }

    // TODO -- this'll eventually go on Envelope itself
    public interface IPersistable
    {
        byte[] Identity();
        byte[] Body();
    }
}

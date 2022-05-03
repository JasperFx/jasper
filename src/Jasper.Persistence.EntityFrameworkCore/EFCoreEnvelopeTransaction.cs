using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Jasper.Persistence.EntityFrameworkCore
{
    public class EFCoreEnvelopeTransaction : IEnvelopeTransaction
    {
        private readonly DbContext _db;
        private readonly IExecutionContext _messaging;
        private DatabaseSettings _settings;
        private int _nodeId;

        public EFCoreEnvelopeTransaction(DbContext db, IExecutionContext messaging)
        {
            if (messaging.Persistence is IDatabaseBackedEnvelopePersistence persistence)
            {
                _settings = persistence.DatabaseSettings;
                _nodeId = persistence.Settings.UniqueNodeId;
            }
            else
            {
                throw new InvalidOperationException(
                    "This Jasper application is not using Database backed message persistence. Please configure the message configuration");
            }

            _db = db;
            _messaging = messaging;
        }

        public async Task PersistAsync(Envelope envelope)
        {
            if (_db.Database.CurrentTransaction == null)
            {
                await _db.Database.BeginTransactionAsync();
            }

            var conn = _db.Database.GetDbConnection();
            var tx = _db.Database.CurrentTransaction.GetDbTransaction();
            var cmd = DatabasePersistence.BuildOutgoingStorageCommand(envelope, envelope.OwnerId, _settings);
            cmd.Transaction = tx;
            cmd.Connection = conn;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task PersistAsync(Envelope[] envelopes)
        {
            if (!envelopes.Any())
            {
                return;
            }

            if (_db.Database.CurrentTransaction == null)
            {
                await _db.Database.BeginTransactionAsync();
            }

            var conn = _db.Database.GetDbConnection();
            var tx = _db.Database.CurrentTransaction.GetDbTransaction();
            var cmd = DatabasePersistence.BuildIncomingStorageCommand(envelopes, _settings);
            cmd.Transaction = tx;
            cmd.Connection = conn;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ScheduleJobAsync(Envelope envelope)
        {
            if (_db.Database.CurrentTransaction == null)
            {
                await _db.Database.BeginTransactionAsync();
            }

            var conn = _db.Database.GetDbConnection();
            var tx = _db.Database.CurrentTransaction.GetDbTransaction();
            var builder = _settings.ToCommandBuilder();
            DatabasePersistence.BuildIncomingStorageCommand(_settings, builder, envelope);
            await builder.ExecuteNonQueryAsync(conn, tx: tx);

        }

        public Task CopyToAsync(IEnvelopeTransaction other)
        {
            throw new NotSupportedException();
        }
    }
}

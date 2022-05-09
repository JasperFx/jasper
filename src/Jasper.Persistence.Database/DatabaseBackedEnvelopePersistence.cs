using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Persistence.Durability;
using Microsoft.Extensions.Logging;
using Weasel.Core;
using Weasel.Core.Migrations;

namespace Jasper.Persistence.Database;

public abstract partial class DatabaseBackedEnvelopePersistence<T> : DatabaseBase<T>,
    IDatabaseBackedEnvelopePersistence, IEnvelopeStorageAdmin where T : DbConnection, new()
{
    protected readonly CancellationToken _cancellation;
    private readonly string _outgoingEnvelopeSql;

    protected DatabaseBackedEnvelopePersistence(DatabaseSettings databaseSettings, AdvancedSettings settings,
        ILogger logger) : base(new MigrationLogger(logger), AutoCreate.CreateOrUpdate, databaseSettings.Migrator,
        "JasperEnvelopeStorage", databaseSettings.ConnectionString!)
    {
        DatabaseSettings = databaseSettings;

        Settings = settings;
        _cancellation = settings.Cancellation;

        var transaction = new DurableStorageSession(databaseSettings, settings.Cancellation);

        Session = transaction;

        _cancellation = settings.Cancellation;
        _deleteIncomingEnvelopeById =
            $"delete from {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} where id = @id";
        _incrementIncominEnvelopeAttempts =
            $"update {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} set attempts = @attempts where id = @id";

        // ReSharper disable once VirtualMemberCallInConstructor
        _outgoingEnvelopeSql = determineOutgoingEnvelopeSql(databaseSettings, settings);
    }

    public AdvancedSettings Settings { get; }

    public DatabaseSettings DatabaseSettings { get; }

    public IEnvelopeStorageAdmin Admin => this;

    public IDurableStorageSession Session { get; }

    public abstract void Describe(TextWriter writer);

    public Task ReassignDormantNodeToAnyNodeAsync(int nodeId)
    {
        var sql = $@"
update {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable}
  set owner_id = 0
where
  owner_id = @owner;

update {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable}
  set owner_id = 0
where
  owner_id = @owner;
";

        return Session.CreateCommand(sql)
            .With("owner", nodeId)
            .ExecuteNonQueryAsync(_cancellation);
    }

    public async Task<int[]> FindUniqueOwnersAsync(int currentNodeId)
    {
        if (Session.Transaction == null)
        {
            throw new InvalidOperationException("No current transaction");
        }

        var sql = $@"
select distinct owner_id from {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} where owner_id != 0 and owner_id != @owner
union
select distinct owner_id from {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where owner_id != 0 and owner_id != @owner";

        var list = await Session.Transaction.CreateCommand(sql)
            .With("owner", currentNodeId)
            .FetchList<int>(_cancellation);

        return list.ToArray();
    }

    public void Dispose()
    {
        Session?.Dispose();
    }
}

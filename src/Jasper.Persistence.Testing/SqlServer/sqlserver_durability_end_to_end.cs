using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using IntegrationTests;
using Jasper;
using Jasper.Attributes;
using Jasper.Persistence.Database;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Marten;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using TestingSupport;
using Weasel.Core;
using Weasel.Postgresql;
using Xunit;


public class sqlserver_durability_end_to_end : IAsyncLifetime
{
    private const string SenderSchemaName = "sender";
    private const string ReceiverSchemaName = "receiver";
    private Uri _listener;

    private LightweightCache<string,IHost> _senders;
    private LightweightCache<string,IHost> _receivers;

    public sqlserver_durability_end_to_end()
    {
        // Stupid work around to way too clever code
        JasperOptions.RememberedApplicationAssembly = GetType().Assembly;
    }

    public async Task InitializeAsync()
    {
        _listener = new Uri($"tcp://localhost:{PortFinder.GetAvailablePort()}");

        await new SqlServerEnvelopePersistence(
                new SqlServerSettings { ConnectionString = Servers.SqlServerConnectionString, SchemaName = ReceiverSchemaName },
                new AdvancedSettings(null), new NullLogger<SqlServerEnvelopePersistence>())
            .RebuildAsync();

        await new SqlServerEnvelopePersistence(
                new SqlServerSettings { ConnectionString = Servers.SqlServerConnectionString, SchemaName = SenderSchemaName },
                new AdvancedSettings(null), new NullLogger<SqlServerEnvelopePersistence>())
            .RebuildAsync();

        await buildTraceDocTable();

        _receivers = new LightweightCache<string, IHost>(key =>
        {
            // This is bootstrapping a Jasper application through the
            // normal ASP.Net Core IWebHostBuilder
            return Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.Handlers.DisableConventionalDiscovery();
                    opts.Handlers.IncludeType<TraceHandler>();

                    opts.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, ReceiverSchemaName);

                    opts.ListenForMessagesFrom(_listener).UsePersistentInbox();
                })
                .Start();
        });

        _senders = new LightweightCache<string, IHost>(key =>
        {
            return Host.CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.Handlers.DisableConventionalDiscovery();

                    opts.Publish(x => x.Message<TraceMessage>().To(_listener)
                        .UsePersistentOutbox());

                    opts.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, SenderSchemaName);

                    opts.Advanced.ScheduledJobPollingTime = 1.Seconds();
                    opts.Advanced.ScheduledJobFirstExecution = 0.Seconds();
                })
                .Start();
        });
    }

    private async Task buildTraceDocTable()
    {
        await using var conn = new SqlConnection(Servers.SqlServerConnectionString);
        await conn.OpenAsync();

        await conn.CreateCommand(@"
IF OBJECT_ID('receiver.trace_doc', 'U') IS NOT NULL
  drop table receiver.trace_doc;

").ExecuteNonQueryAsync();

        await conn.CreateCommand(@"
create table receiver.trace_doc
(
	id uniqueidentifier not null
		primary key,
	name varchar(100) not null
);

").ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        foreach (var host in _receivers.GetAll())
        {
            await host.StopAsync();
        }

        _receivers.ClearAll();

        foreach (var host in _senders)
        {
            await host.StopAsync();
        }

        _senders.ClearAll();
    }

    protected void StartReceiver(string name)
    {
        _receivers.FillDefault(name);
    }

    protected void StartSender(string name)
    {
        _senders.FillDefault(name);
    }

    protected ValueTask SendFrom(string sender, string name)
    {
        return _senders[sender].Services.GetRequiredService<IMessageContext>().SendAsync(new TraceMessage { Name = name });
    }

    protected async Task SendMessages(string sender, int count)
    {
        var runtime = _senders[sender];

        for (var i = 0; i < count; i++)
        {
            var msg = new TraceMessage { Name = Guid.NewGuid().ToString() };
            await runtime.Services.GetRequiredService<IMessageContext>().SendAsync(msg);
        }
    }

    protected int ReceivedMessageCount()
    {
        using var conn = new SqlConnection(Servers.SqlServerConnectionString);
        conn.Open();
        return (int)conn.CreateCommand("select count(*) from receiver.trace_doc").ExecuteScalar();
    }

    protected async Task WaitForMessagesToBeProcessed(int count)
    {
        await using var conn = new SqlConnection(Servers.SqlServerConnectionString);
        await conn.OpenAsync();

        for (var i = 0; i < 200; i++)
        {
            var actual = (int)conn.CreateCommand("select count(*) from receiver.trace_doc").ExecuteScalar();
            var envelopeCount = PersistedIncomingCount();

            Trace.WriteLine($"waitForMessages: {actual} actual & {envelopeCount} incoming envelopes");

            if (actual == count && envelopeCount == 0)
            {
                return;
            }

            await Task.Delay(250);
        }

        throw new Exception("All messages were not received");
    }

    protected long PersistedIncomingCount()
    {
        using var conn = new SqlConnection(Servers.SqlServerConnectionString);
        conn.Open();

        return (int)conn.CreateCommand(
                $"select count(*) from receiver.{DatabaseConstants.IncomingTable}")
            .ExecuteScalar();
    }

    protected long PersistedOutgoingCount()
    {
        using var conn = new SqlConnection(Servers.SqlServerConnectionString);
        conn.Open();

        return (int)conn.CreateCommand(
                $"select count(*) from sender.{DatabaseConstants.OutgoingTable}")
            .ExecuteScalar();
    }

    protected async Task StopReceiver(string name)
    {
        var receiver = _receivers[name];
        await receiver.StopAsync();
        receiver.Dispose();
        _receivers.Remove(name);
    }

    protected async Task StopSender(string name)
    {
        var sender = _senders[name];
        await sender.StopAsync();
        sender.Dispose();
        _senders.Remove(name);
    }

    [Fact]
    public async Task sending_recovered_messages_when_sender_starts_up()
    {
        StartSender("Sender1");
        await SendMessages("Sender1", 10);
        await StopSender("Sender1");
        PersistedOutgoingCount().ShouldBe(10);
        StartReceiver("Receiver1");
        StartSender("Sender2");
        await WaitForMessagesToBeProcessed(10);
        PersistedIncomingCount().ShouldBe(0);
        PersistedOutgoingCount().ShouldBe(0);
        ReceivedMessageCount().ShouldBe(10);
    }

    [Fact]
    public async Task sending_resumes_when_the_receiver_is_detected()
    {
        StartSender("Sender1");
        await SendMessages("Sender1", 5);
        StartReceiver("Receiver1");
        await WaitForMessagesToBeProcessed(5);
        PersistedIncomingCount().ShouldBe(0);
        PersistedOutgoingCount().ShouldBe(0);
        ReceivedMessageCount().ShouldBe(5);
    }
}

public class TraceDoc
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
}

public class TraceMessage
{
    public string Name { get; set; }
}

[JasperIgnore]
public class TraceHandler
{
    [Transactional]
    public void Handle(TraceMessage message, SqlTransaction tx)
    {
        var traceDoc = new TraceDoc { Name = message.Name };

        tx.CreateCommand("insert into receiver.trace_doc (id, name) values (@id, @name)")
            .With("id", traceDoc.Id)
            .With("name", traceDoc.Name)
            .ExecuteNonQuery();
    }
}

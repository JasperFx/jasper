using IntegrationTests;
using Jasper;
using Jasper.Attributes;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Marten;
using Microsoft.Extensions.Hosting;

namespace ScheduledJobTests.Postgresql;

[Collection("marten")]
public class MartenDurabilityCompliance : DurabilityComplianceContext<TriggerMessageReceiver, ItemCreatedHandler>
{

    protected override void configureReceiver(JasperOptions receiverOptions)
    {
        receiverOptions.Services.AddMarten(opts =>
        {
            opts.Connection(Servers.PostgresConnectionString);
            opts.DatabaseSchemaName = "outbox_receiver";
        }).IntegrateWithJasper();

    }

    protected override void configureSender(JasperOptions senderOptions)
    {
        senderOptions.Services.AddMarten(opts =>
        {
            opts.Connection(Servers.PostgresConnectionString);
            opts.DatabaseSchemaName = "outbox_sender";
        }).IntegrateWithJasper();

    }

    protected override ItemCreated loadItem(IHost receiver, Guid id)
    {
        using (var session = receiver.Get<IDocumentStore>().QuerySession())
        {
            return session.Load<ItemCreated>(id);
        }
    }


    protected override async Task withContext(IHost sender, IExecutionContext context,
        Func<IExecutionContext, ValueTask> action)
    {
        var senderStore = sender.Get<IDocumentStore>();

        await using var session = senderStore.LightweightSession();
        await context.EnlistInOutboxAsync(session);

        await action(context);

        await session.SaveChangesAsync();
    }

    protected override IReadOnlyList<Envelope> loadAllOutgoingEnvelopes(IHost sender)
    {
        var admin = sender.Get<IEnvelopePersistence>().Admin;

        return admin.AllOutgoingAsync().GetAwaiter().GetResult();
    }
}

public class TriggerMessageReceiver
{
    [Transactional]
    public ValueTask Handle(TriggerMessage message, IDocumentSession session, IExecutionContext context)
    {
        var response = new CascadedMessage
        {
            Name = message.Name
        };

        return context.RespondToSenderAsync(response);
    }
}

public class ItemCreatedHandler
{
    [Transactional]
    public static void Handle(ItemCreated created, IDocumentSession session,
        Envelope envelope)
    {
        session.Store(created);
    }
}

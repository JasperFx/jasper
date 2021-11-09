using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Marten;
using Microsoft.Extensions.Hosting;
using Polly;
using Xunit;

namespace Jasper.Persistence.Testing.Marten.Durability
{
    [Collection("marten")]
    public class durability_specs : DurableFixture<TriggerMessageReceiver, ItemCreatedHandler>
    {
        protected override void configureReceiver(JasperOptions receiverOptions)
        {
            receiverOptions.Extensions.UseMarten(marten =>
            {
                marten.Connection(Servers.PostgresConnectionString);
                marten.DatabaseSchemaName = "outbox_receiver";
            });
        }

        protected override void configureSender(JasperOptions senderOptions)
        {
            senderOptions.UseMarten(marten =>
            {
                marten.Connection(Servers.PostgresConnectionString);
                marten.DatabaseSchemaName = "outbox_sender";
            });
        }


        protected override ItemCreated loadItem(IHost receiver, Guid id)
        {
            using (var session = receiver.Get<IDocumentStore>().QuerySession())
            {
                return session.Load<ItemCreated>(id);
            }
        }


        protected override async Task withContext(IHost sender, IExecutionContext context,
            Func<IExecutionContext, Task> action)
        {
            var senderStore = sender.Get<IDocumentStore>();

            using (var session = senderStore.LightweightSession())
            {
                await context.EnlistInTransaction(session);

                await action(context);

                await session.SaveChangesAsync();
            }
        }

        protected override IReadOnlyList<Envelope> loadAllOutgoingEnvelopes(IHost sender)
        {
            var admin = sender.Get<IEnvelopePersistence>().Admin;
            return admin.AllOutgoingEnvelopes().GetAwaiter().GetResult();
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Jasper.Persistence.EntityFrameworkCore
{
    public static class JasperEntityFrameworkCoreConfigurationExtensions
    {
        /// <summary>
        /// Uses Entity Framework Core for Saga persistence and transactional
        /// middleware
        /// </summary>
        /// <param name="extensions"></param>
        public static void UseEntityFrameworkCorePersistence(this IExtensions extensions)
        {
            extensions.Include<EntityFrameworkCoreBackedPersistence>();
        }
    }

    public static class JasperEnvelopeEntityFrameworkCoreExtensions
    {
        public static async Task SaveChangesAndFlushMessages(this DbContext context, IMessageContext messages, CancellationToken cancellation = default)
        {
            await context.SaveChangesAsync(cancellation);
            var tx = context.Database.CurrentTransaction?.GetDbTransaction();
            if (tx != null)
            {
                await tx.CommitAsync(cancellation);
            }

            await messages.SendAllQueuedOutgoingMessages();
        }

        /// <summary>
        /// Enlists the current IMessagingContext in the EF Core DbContext's transaction
        /// lifecycle. Note, you will need to call IMessageContext.SendAllQueuedOutgoingMessages()
        /// to actually have the outgoing messages sent
        /// </summary>
        /// <param name="messaging"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static Task EnlistInTransaction(this IMessageContext messaging, DbContext dbContext)
        {
            var transaction = new EFCoreEnvelopeTransaction(dbContext, messaging);
            return messaging.EnlistInTransaction(transaction);
        }



        public static void MapEnvelopeStorage(this ModelBuilder builder, string schemaName = "dbo")
        {
            builder.Entity<OutgoingEnvelope>(map =>
            {
                map.ToTable(string.IsNullOrEmpty(schemaName) || schemaName.EqualsIgnoreCase("dbo") ? DatabaseConstants.OutgoingTable : $"{schemaName}.{DatabaseConstants.OutgoingTable}");
                map.HasKey(x => x.Id);
                map.Property(x => x.OwnerId).HasColumnName("owner_id");
                map.Property(x => x.Destination).HasColumnName("destination");
                map.Property(x => x.DeliverBy).HasColumnName("deliver_by");
                map.Property(x => x.Body).HasColumnName("body");
            });

            builder.Entity<DeadLetterEnvelope>(map =>
            {
                map.ToTable(string.IsNullOrEmpty(schemaName) || schemaName.EqualsIgnoreCase("dbo") ? DatabaseConstants.DeadLetterTable : $"{schemaName}.{DatabaseConstants.DeadLetterTable}");
                map.HasKey(x => x.Id);
                map.Property(x => x.Source).HasColumnName("source");
                map.Property(x => x.MessageType).HasColumnName("message_type");
                map.Property(x => x.Explanation).HasColumnName("explanation");
                map.Property(x => x.ExceptionText).HasColumnName("exception_text");
                map.Property(x => x.ExceptionType).HasColumnName("exception_type");
                map.Property(x => x.ExceptionMessage).HasColumnName("exception_message");
                map.Property(x => x.Body).HasColumnName("body");
            });
        }
    }
}

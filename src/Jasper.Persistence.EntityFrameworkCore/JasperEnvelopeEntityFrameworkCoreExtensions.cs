using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Jasper.Persistence.EntityFrameworkCore
{
    public static class JasperEnvelopeEntityFrameworkCoreExtensions
    {
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
            builder.Entity<IncomingEnvelope>(map =>
            {
                map.ToTable($"jasper_incoming_envelopes");
                map.HasKey(x => x.Id);
                map.Property(x => x.OwnerId).HasColumnName("owner_id");
                map.Property(x => x.Status).HasColumnName("status");
                map.Property(x => x.ExecutionTime).HasColumnName("execution_time");
                map.Property(x => x.Attempts).HasColumnName("attempts");
                map.Property(x => x.Body).HasColumnName("body");
            });

            builder.Entity<OutgoingEnvelope>(map =>
            {
                map.ToTable($"jasper_outgoing_envelopes");
                map.HasKey(x => x.Id);
                map.Property(x => x.OwnerId).HasColumnName("owner_id");
                map.Property(x => x.Destination).HasColumnName("destination");
                map.Property(x => x.DeliverBy).HasColumnName("deliver_by");
                map.Property(x => x.Body).HasColumnName("body");
            });

            builder.Entity<DeadLetterEnvelope>(map =>
            {
                map.ToTable($"{schemaName}.jasper_dead_letters");
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

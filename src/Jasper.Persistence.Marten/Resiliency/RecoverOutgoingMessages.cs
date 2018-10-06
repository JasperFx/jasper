using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Persistence.Marten.Persistence.Operations;
using Jasper.Util;
using Marten;
using Marten.Util;
using Npgsql;
using NpgsqlTypes;

namespace Jasper.Persistence.Marten.Resiliency
{
    public class RecoverOutgoingMessages : IMessagingAction
    {
        public static readonly int OutgoingMessageLockId = "recover-outgoing-messages".GetHashCode();
        private readonly ISubscriberGraph _subscribers;
        private readonly EnvelopeTables _marker;
        private readonly ITransportLogger _logger;
        private readonly MessagingSettings _settings;
        private readonly string _findUniqueDestinations;
        private readonly string _findOutgoingEnvelopesSql;
        private readonly string _deleteOutgoingSql;

        public RecoverOutgoingMessages(ISubscriberGraph subscribers, MessagingSettings settings, EnvelopeTables marker,
            ITransportLogger logger)
        {
            _subscribers = subscribers;
            _settings = settings;
            _marker = marker;
            _logger = logger;

            _findUniqueDestinations = $"select distinct destination from {_marker.Outgoing}";
            _findOutgoingEnvelopesSql = $"select body from {marker.Outgoing} where owner_id = {TransportConstants.AnyNode} and destination = :destination limit {settings.Retries.RecoveryBatchSize}";
            _deleteOutgoingSql = $"delete from {marker.Outgoing} where owner_id = :owner and destination = :destination";

        }

        public async Task<List<Uri>> FindAllOutgoingDestinations(IDocumentSession session)
        {
            var list = new List<Uri>();

            var cmd = session.Connection.CreateCommand(_findUniqueDestinations);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var text = await reader.GetFieldValueAsync<string>(0);
                    list.Add(text.ToUri());
                }
            }

            return list;
        }

        public async Task Execute(IDocumentSession session, ISchedulingAgent agent)
        {
            if (!await session.TryGetGlobalTxLock(OutgoingMessageLockId))
                return;


            var destinations = await FindAllOutgoingDestinations(session);

            var count = 0;
            foreach (var destination in destinations)
            {
                count += await recoverFrom(destination, session);
            }

            var wasMaxedOut = count >= _settings.Retries.RecoveryBatchSize;

            if (wasMaxedOut)
            {
                agent.RescheduleOutgoingRecovery();
            }
        }

        private async Task<int> recoverFrom(Uri destination, IDocumentSession session)
        {


            try
            {
                var channel = _subscribers.GetOrBuild(destination);

                if (channel.Latched) return 0;

                Trace.WriteLine($"Looking for dormant outgoing messages to {destination} in {_settings.ServiceName}");
                var outgoing = await session.Connection.CreateCommand(_findOutgoingEnvelopesSql)
                    .With("destination", destination.ToString(), NpgsqlDbType.Varchar)
                    .ExecuteToEnvelopes();

                if (outgoing.Count == 0)
                {
                    return 0;
                }

                Trace.WriteLine($"Recovered {outgoing.Count} outgoing messages to {destination}");

                var filtered = filterExpired(session, outgoing);


                // Might easily try to do this in the time between starting
                // and having the data fetched. Was able to make that happen in
                // (contrived) testing
                if (channel.Latched || !filtered.Any()) return 0;

                Trace.WriteLine($"After filtering, taking ownership of {filtered.Length} outgoing messages to {destination} assigned to node {_marker.CurrentNodeId}");

                session.MarkOwnership(_marker.Outgoing, _marker.CurrentNodeId, filtered);

                await session.SaveChangesAsync();

                _logger.RecoveredOutgoing(filtered);

                foreach (var envelope in filtered)
                {
                    try
                    {
                        await channel.QuickSend(envelope);
                    }
                    catch (Exception e)
                    {
                        _logger.LogException(e, message:$"Unable to enqueue {envelope} for sending");
                    }
                }

                return outgoing.Count();

            }
            catch (UnknownTransportException e)
            {
                _logger.LogException(e, message: $"Could not resolve a channel for {destination}");

                await DeleteFromOutgoingEnvelopes(session, TransportConstants.AnyNode, destination);
                await session.SaveChangesAsync();

                return 0;
            }
        }

        public Task DeleteFromOutgoingEnvelopes(IDocumentSession session, int ownerId, Uri destination)
        {
            return session.Connection.CreateCommand(_deleteOutgoingSql)
                .With("destination", destination.ToString(), NpgsqlDbType.Varchar)
                .With("owner", ownerId, NpgsqlDbType.Integer).ExecuteNonQueryAsync();
        }


        private Envelope[] filterExpired(IDocumentSession session, IEnumerable<Envelope> outgoing)
        {
            var expiredMessages = outgoing.Where(x => x.IsExpired()).ToArray();
            _logger.DiscardedExpired(expiredMessages);

            session.DeleteEnvelopes(_marker.Outgoing, expiredMessages);

            return outgoing.Where(x => !x.IsExpired()).ToArray();
        }
    }

}

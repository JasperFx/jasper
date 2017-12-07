using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;
using Marten;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class RecoverOutgoingMessages : IMessagingAction
    {
        public static readonly int OutgoingMessageLockId = "recover-incoming-messages".GetHashCode();
        private readonly IChannelGraph _channels;
        private readonly OwnershipMarker _marker;
        private readonly ISchedulingAgent _schedulingAgent;
        private readonly CompositeTransportLogger _logger;
        private readonly BusSettings _settings;

        public RecoverOutgoingMessages(IChannelGraph channels, BusSettings settings, OwnershipMarker marker, ISchedulingAgent schedulingAgent, CompositeTransportLogger logger)
        {
            _channels = channels;
            _settings = settings;
            _marker = marker;
            _schedulingAgent = schedulingAgent;
            _logger = logger;
        }

        public async Task Execute(IDocumentSession session)
        {
            if (!await session.TryGetGlobalTxLock(OutgoingMessageLockId))
                return;


            // TODO -- turn this into a compiled query if it's usable
            var destinations = await session.Query<Envelope>().Where(x =>
                    x.Status == TransportConstants.Outgoing && x.OwnerId == TransportConstants.AnyNode)
                .Select(x => x.Address).Distinct().ToListAsync();

            var count = 0;
            foreach (var destination in destinations.Select(x => x.ToUri()))
            {
                count += await recoverFrom(destination, session);
            }

            var wasMaxedOut = count >= _settings.Retries.RecoveryBatchSize;

            if (wasMaxedOut)
            {
                _schedulingAgent.RescheduleOutgoingRecovery();
            }
        }

        private async Task<int> recoverFrom(Uri destination, IDocumentSession session)
        {
            try
            {
                var channel = _channels.GetOrBuildChannel(destination);

                if (channel.Latched) return 0;

                var outgoing = (await session.QueryAsync(new FindOutgoingEnvelopesByDestination
                {
                    PageSize = _settings.Retries.RecoveryBatchSize,
                    Address = destination.ToString()
                }));

                var filtered = filterExpired(session, outgoing);

                // Might easily try to do this in the time between starting
                // and having the data fetched. Was able to make that happen in
                // (contrived) testing
                if (channel.Latched || !filtered.Any()) return 0;

                await _marker.MarkOutgoingOwnedByThisNode(session, filtered);

                await session.SaveChangesAsync();

                _logger.RecoveredOutgoing(filtered);

                // TODO -- will need a compensating action here if any of this fails
                foreach (var envelope in filtered)
                {
                    await channel.QuickSend(envelope);
                }

                return outgoing.Count();

            }
            catch (UnknownTransportException e)
            {
                _logger.LogException(e, message: $"Could not resolve a channel for {destination}");

                session.DeleteWhere<Envelope>(x => x.Status == TransportConstants.Outgoing && x.Address == destination.ToString() && x.OwnerId == TransportConstants.AnyNode);
                await session.SaveChangesAsync();

                return 0;
            }
        }


        private Envelope[] filterExpired(IDocumentSession session, IEnumerable<Envelope> outgoing)
        {
            var expiredMessages = outgoing.Where(x => x.IsExpired()).ToArray();
            _logger.DiscardedExpired(expiredMessages);

            foreach (var expired in expiredMessages)
            {
                session.Delete(expired);
            }

            return outgoing.Where(x => !x.IsExpired()).ToArray();
        }
    }
}

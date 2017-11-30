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

            var outgoing = await findPersistedOutgoingEnvelopes(session);

            if (!outgoing.Any()) return;

            bool wasMaxedOut = outgoing.Count == _settings.Retries.RecoveryBatchSize;

            // Delete any envelope that is expired by its DeliveryBy value
            var filtered = filterExpired(session, outgoing);

            if (filtered.Any())
            {
                await sendOutgoing(session, filtered);
            }
            else
            {
                // Just commit the expired message delivery
                await session.SaveChangesAsync();
            }

            if (wasMaxedOut)
            {
                _schedulingAgent.RescheduleOutgoingRecovery();
            }
        }

        private async Task<List<Envelope>> findPersistedOutgoingEnvelopes(IDocumentSession session)
        {
            var latchedSenders = _channels.AllKnownChannels().Where(x => x.Latched).Select(x => x.Uri.ToString())
                .ToArray();

            if (latchedSenders.Any())
            {
                return (await session.QueryAsync(new FindOutgoingEnvelopes()
                {
                    PageSize = _settings.Retries.RecoveryBatchSize,
                    Latched = latchedSenders
                })).ToList();
            }

            // It's a List behind the covers, but to get R# to shut up, I did the ToArray()
            return (await session.QueryAsync(new FindAtLargeEnvelopes
            {
                Status = TransportConstants.Outgoing,
                PageSize = _settings.Retries.RecoveryBatchSize
            })).ToList();
        }

        private async Task sendOutgoing(IDocumentSession session, Envelope[] outgoing)
        {
            await _marker.MarkOutgoingOwnedByThisNode(session, outgoing);


            await session.SaveChangesAsync();

            _logger.RecoveredOutgoing(outgoing);

            var groups = outgoing.GroupBy(x => x.Destination);
            foreach (var @group in groups)
            {
                try
                {
                    var channel = _channels.GetOrBuildChannel(@group.Key);

                    if (channel.Latched)
                    {
                        // TODO -- may need to be retried
                        await _marker.MarkOwnedByAnyNode(session, @group.ToArray());
                        await session.SaveChangesAsync();
                    }
                    else
                    {
                        foreach (var envelope in @group)
                        {
#pragma warning disable 4014
                            channel.QuickSend(envelope);
#pragma warning restore 4014
                        }
                    }
                }
                catch (UnknownTransportException e)
                {
                    _logger.DiscardedUnknownTransport(@group);

                    // TODO -- may need to be retried
                    var ids = @group.Select(x => x.Id).ToArray();
                    session.DeleteWhere<Envelope>(x => x.Id.IsOneOf(ids));
                    await session.SaveChangesAsync();

                }
            }

        }

        private Envelope[] filterExpired(IDocumentSession session, List<Envelope> outgoing)
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

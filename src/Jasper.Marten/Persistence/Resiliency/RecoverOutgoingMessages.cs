using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Marten;

namespace Jasper.Marten.Persistence.Resiliency
{
    // TODO -- THIS HAS NOT BEEN TESTED YET
    public class RecoverOutgoingMessages : IMessagingAction
    {
        public static readonly int OutgoingMessageLockId = "recover-incoming-messages".GetHashCode();
        private readonly IChannelGraph _channels;
        private readonly OwnershipMarker _marker;
        private readonly ISchedulingAgent _schedulingAgent;
        private readonly BusSettings _settings;

        public RecoverOutgoingMessages(IChannelGraph channels, BusSettings settings, OwnershipMarker marker,
            ISchedulingAgent schedulingAgent)
        {
            _channels = channels;
            _settings = settings;
            _marker = marker;
            _schedulingAgent = schedulingAgent;
        }

        public async Task Execute(IDocumentSession session)
        {
            if (!await session.TryGetGlobalTxLock(OutgoingMessageLockId))
                return;


            // Have it loop until either a time limit has been reached or a certain
            // queue count has been reached
            // TODO -- how many do you pull in here? Is it configurable?

            // TODO -- take advantage of channels that are latched to filter

            // It's a List behind the covers, but to get R# to shut up, I did the ToArray()
            var outgoing = (await session.QueryAsync(new FindAtLargeEnvelopes
            {
                Status = TransportConstants.Outgoing
            })).ToArray();

            // TODO -- filter out ones that are expired


            // TODO -- do you throw away messages that cannot be matched to an outgoing channel?
            // I say we make an "unknown channel" here that can just collect the message
            // and put it into some kind of Undeliverable status

            var groups = outgoing
                .GroupBy(x => x.Destination)
                .Select(x => new {Channel = _channels.GetOrBuildChannel(x.Key), Envelopes = x.ToArray()})
                .Where(x => !x.Channel.Latched).ToArray();

            // Doing it this way results in fewer network round trips to the DB
            // vs looping through the groups
            var all = groups.SelectMany(x => x.Envelopes).ToArray();
            await _marker.MarkOutgoingOwnedByThisNode(session, all);

            foreach (var group in groups)
            foreach (var envelope in group.Envelopes)
                await group.Channel.QuickSend(envelope);


            await session.SaveChangesAsync();

            // TODO -- determine if it should schedule another outgoing recovery batch immediately
        }
    }
}

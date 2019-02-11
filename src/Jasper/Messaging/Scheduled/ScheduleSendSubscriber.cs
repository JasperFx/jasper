using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging.Scheduled
{
    public class ScheduleSendSubscriber : ISubscriber
    {
        private readonly MessageContext _context;

        public ScheduleSendSubscriber(MessageContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
        }

        public Uri Uri { get; } = TransportConstants.DurableLoopbackUri;
        public Uri ReplyUri { get; }= TransportConstants.DurableLoopbackUri;
        public bool Latched { get; } = false;
        public bool IsDurable { get; } = true;
        public int QueuedCount { get; } = 0;
        public string[] ContentTypes { get; set; } = new string[]{TransportConstants.SerializedEnvelope};
        public bool SupportsNativeScheduledSend { get; } = true;

        public bool ShouldSendMessage(Type messageType)
        {
            return true;
        }

        public Task Send(Envelope envelope)
        {
            return _context.ScheduleEnvelope(envelope);
        }

        public Task QuickSend(Envelope envelope)
        {
            return _context.ScheduleEnvelope(envelope);
        }
    }
}

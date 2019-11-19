using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Scheduled
{
    public static class EnvelopeScheduleExtensions
    {
        public static Envelope ForScheduledSend(this Envelope envelope, ISendingAgent sender)
        {
            envelope.EnsureData();

            return new Envelope(envelope, EnvelopeReaderWriter.Instance)
            {
                Message = envelope,
                MessageType = TransportConstants.ScheduledEnvelope,
                ExecutionTime = envelope.ExecutionTime,
                ContentType = TransportConstants.SerializedEnvelope,
                Destination = TransportConstants.DurableLoopbackUri,
                Status = TransportConstants.Scheduled,
                OwnerId = TransportConstants.AnyNode,
                Sender = sender
            };
        }
    }
}

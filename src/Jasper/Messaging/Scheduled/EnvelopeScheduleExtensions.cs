using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging.Scheduled
{
    public static class EnvelopeScheduleExtensions
    {
        public static Envelope ForScheduledSend(this Envelope envelope, ISubscriber subscriber)
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
                Subscriber = subscriber
            };
        }
    }
}

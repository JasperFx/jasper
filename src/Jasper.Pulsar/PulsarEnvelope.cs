using System.Buffers;
using DotPulsar.Abstractions;

namespace Jasper.Pulsar
{
    public class PulsarEnvelope : Envelope
    {
        public IMessage<ReadOnlySequence<byte>> MessageData { get; }

        public PulsarEnvelope(IMessage<ReadOnlySequence<byte>> messageData)
        {
            MessageData = messageData;
        }
    }
}

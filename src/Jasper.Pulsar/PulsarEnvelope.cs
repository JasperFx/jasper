using System.Buffers;
using DotPulsar.Abstractions;

namespace Jasper.Pulsar
{
    public class PulsarEnvelope : Envelope
    {
        public IMessage<ReadOnlySequence<byte>> Message { get; }

        public PulsarEnvelope(IMessage<ReadOnlySequence<byte>> message)
        {
            Message = message;
        }
    }
}

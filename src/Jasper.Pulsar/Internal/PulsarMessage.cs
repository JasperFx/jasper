using System.Buffers;
using DotPulsar;

namespace Jasper.Pulsar.Internal
{
    internal class PulsarMessage
    {
        public MessageMetadata Metadata { get; }
        public ReadOnlySequence<byte> Data { get; }

        public PulsarMessage(ReadOnlySequence<byte> data, MessageMetadata metadata)
        {
            Data = data;
            Metadata = metadata;
        }

        public PulsarMessage(byte[] data, MessageMetadata metadata) : this(new ReadOnlySequence<byte>(data), metadata)
        {
        }
    }
}

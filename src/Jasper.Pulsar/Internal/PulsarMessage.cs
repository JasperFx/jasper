using System.Buffers;
using System.Collections.Generic;
using DotPulsar;

namespace Jasper.Pulsar.Internal
{
    internal class PulsarMessage
    {
        public MessageMetadata Metadata { get; } = new MessageMetadata();
        public ReadOnlySequence<byte> Data { get; }
        public IReadOnlyDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        public PulsarMessage(ReadOnlySequence<byte> data)
        {
            Data = data;
        }

        public PulsarMessage(ReadOnlySequence<byte> data, MessageMetadata metadata)
        {
            Data = data;
            Metadata = metadata;
        }

        public PulsarMessage(byte[] data, MessageMetadata metadata) : this(new ReadOnlySequence<byte>(data), metadata)
        {
        }

        public PulsarMessage(ReadOnlySequence<byte> data, IReadOnlyDictionary<string, string> properties) : this(data)
        {
            Properties = properties;
        }
    }
}

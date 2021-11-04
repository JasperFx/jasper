using System.Buffers;
using System.Collections.Generic;
using DotPulsar;

namespace Jasper.DotPulsar.Internal
{
    internal class DotPulsarMessage
    {
        public MessageMetadata Metadata { get; } = new MessageMetadata();
        public ReadOnlySequence<byte> Data { get; }
        public IReadOnlyDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        public DotPulsarMessage(ReadOnlySequence<byte> data)
        {
            Data = data;
        }

        public DotPulsarMessage(ReadOnlySequence<byte> data, MessageMetadata metadata)
        {
            Data = data;
            Metadata = metadata;
        }

        public DotPulsarMessage(byte[] data, MessageMetadata metadata) : this(new ReadOnlySequence<byte>(data), metadata)
        {
        }

        public DotPulsarMessage(ReadOnlySequence<byte> data, IReadOnlyDictionary<string, string> properties) : this(data)
        {
            Properties = properties;
        }
    }
}

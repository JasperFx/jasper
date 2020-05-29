using System.Buffers;
using System.Collections.Generic;
using DotPulsar;

namespace Jasper.Pulsar.Internal
{
    internal class PulsarMessage
    {
        public MessageId MessageId { get; set; }
        public MessageMetadata Metadata { get; } = new MessageMetadata();
        public ReadOnlySequence<byte> Data { get; }
        public IReadOnlyDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        public PulsarMessage(Message message)
        {
            MessageId = message.MessageId;
            Metadata.Key = message.Key;
            Metadata.SequenceId = message.SequenceId;
            Metadata.EventTime = message.EventTime;
            Metadata.EventTimeAsDateTimeOffset = message.EventTimeAsDateTimeOffset;
            Metadata.KeyBytes = message.KeyBytes;
            Metadata.OrderingKey = message.OrderingKey;

            Properties = message.Properties;
            Data = message.Data;
        }

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

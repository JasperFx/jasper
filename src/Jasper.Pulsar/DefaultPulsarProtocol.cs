using System.Buffers;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.Transports;

namespace Jasper.Pulsar
{
    public class DefaultPulsarProtocol : Protocol<IMessage<ReadOnlySequence<byte>>, MessageMetadata>, IPulsarProtocol
    {
        protected override void writeOutgoingHeader(MessageMetadata outgoing, string key, string value)
        {
            outgoing[key] = value;
        }

        protected override bool tryReadIncomingHeader(IMessage<ReadOnlySequence<byte>> incoming, string key, out string value)
        {
            return incoming.Properties.TryGetValue(key, out value);
        }

        protected override void writeIncomingHeaders(IMessage<ReadOnlySequence<byte>> incoming, Envelope envelope)
        {
            foreach (var pair in incoming.Properties)
            {
                envelope.Headers[pair.Key] = pair.Value;
            }
        }
    }
}

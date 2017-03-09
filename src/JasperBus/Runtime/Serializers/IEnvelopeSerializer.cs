using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Baseline;
using JasperBus.Configuration;

namespace JasperBus.Runtime.Serializers
{
    public interface IEnvelopeSerializer
    {
        object Deserialize(Envelope envelope, ChannelNode node);
        void Serialize(Envelope envelope, ChannelNode node);
    }

    public class EnvelopeSerializer : IEnvelopeSerializer
    {
        private readonly ChannelGraph _graph;
        private readonly Dictionary<string, IMessageSerializer> _serializers = new Dictionary<string, IMessageSerializer>();

        public EnvelopeSerializer(ChannelGraph graph, IEnumerable<IMessageSerializer> serializers)
        {
            _graph = graph;
            foreach (var serializer in serializers)
            {
                _serializers.SmartAdd(serializer.ContentType, serializer);
            }
        }

        public object Deserialize(Envelope envelope, ChannelNode node)
        {
            throw new System.NotImplementedException();
        }

        public void Serialize(Envelope envelope, ChannelNode node)
        {
            throw new System.NotImplementedException();
        }

        public IMessageSerializer SelectSerializer(Envelope envelope, ChannelNode node)
        {
            if (envelope.ContentType.IsNotEmpty())
            {
                // TODO -- what if it can't be found?
                return _serializers[envelope.ContentType];
            }

            var mimeType = chooseContentType(envelope)
                           ?? chooseContentType(node)
                           ?? chooseContentType(_graph);


            return mimeType.IsEmpty()
                ? null
                : _serializers[mimeType];
        }

        private string chooseContentType(IContentTypeAware level)
        {
            return level.Accepts.FirstOrDefault(x => _serializers.ContainsKey(x));
        }
    }
}
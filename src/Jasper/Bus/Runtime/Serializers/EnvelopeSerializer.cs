using System;
using System.IO;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Conneg;

namespace Jasper.Bus.Runtime.Serializers
{
    public class EnvelopeSerializer : IEnvelopeSerializer
    {
        private readonly ChannelGraph _channels;
        private readonly SerializationGraph _serializers;

        public EnvelopeSerializer(ChannelGraph channels, SerializationGraph serializers)
        {
            _channels = channels;
            _serializers = serializers;
        }



        public object Deserialize(Envelope envelope, ChannelNode node)
        {
            var contentType = envelope.ContentType ?? node.AcceptedContentTypes.FirstOrDefault();

            if (contentType.IsEmpty())
            {
                throw new EnvelopeDeserializationException($"No content type can be determined for {envelope}");
            }

            if (envelope.Data == null || envelope.Data.Length == 0)
            {
                throw new EnvelopeDeserializationException($"No data on the Envelope");
            }

            if (envelope.MessageType.IsNotEmpty())
            {
                var reader = _serializers.ReaderFor(envelope.MessageType);
                if (reader.HasAnyReaders)
                {
                    try
                    {
                        if (reader.TryRead(envelope.ContentType, envelope.Data, out object model))
                        {
                            return model;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw EnvelopeDeserializationException.ForReadFailure(envelope, ex);
                    }
                }
            }


            throw new EnvelopeDeserializationException($"Unknown content-type '{contentType}' and message-type '{envelope.MessageType}'");
        }

        public void Serialize(Envelope envelope, ChannelNode node)
        {
            if (envelope.Message == null)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Message cannot be null");
            }

            var writer = _serializers.WriterFor(envelope.Message.GetType());

            // TODO -- change the node AcceptedContentTypes to an accepts string instead


            var accepts = envelope.Accepts ?? node?.AcceptedContentTypes.Join(",");
            if (accepts.IsEmpty())
            {
                accepts = _channels.DefaultContentType;
            }


            if (writer.TryWrite(accepts, envelope.Message, out string contentType, out byte[] data))
            {
                envelope.Data = data;
                envelope.ContentType = contentType;

                return;
            }

            throw new InvalidOperationException($"Unable to choose a serializer for {envelope} with content-type {envelope.ContentType} and message type {envelope.Message.GetType().FullName} for channel {node.Uri}");
        }

        public ISerializer SelectSerializer(Envelope envelope, ChannelNode node)
        {
            throw new NotImplementedException("Redo the specs for this?");

//            if (envelope.ContentType.IsNotEmpty())
//            {
//                return _serializers.ContainsKey(envelope.ContentType) ? _serializers[envelope.ContentType] : null;
//            }
//
//            string mimeType = null;
//            if (envelope.AcceptedContentTypes.Any())
//            {
//                mimeType = chooseContentType(envelope);
//            }
//            // It's perfectly possible to not have a matching, static ChannelNode here
//            else if (node != null && node.AcceptedContentTypes.Any())
//            {
//                mimeType = chooseContentType(node);
//            }
//            else
//            {
//                mimeType = chooseContentType(_channels);
//            }
//
//            return mimeType.IsEmpty()
//                ? null
//                : _serializers[mimeType];
        }

//        private string chooseContentType(IContentTypeAware level)
//        {
//            return level.Accepts.FirstOrDefault(x => _serializers.ContainsKey(x));
//        }
    }
}

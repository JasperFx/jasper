using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using Jasper.Conneg;
using Jasper.Conneg.Json;
using Jasper.Messaging.Model;
using Jasper.Util;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Jasper.Messaging.Runtime.Serializers
{
    public class MessagingSerializationGraph : SerializationGraph<IMessageDeserializer, IMessageSerializer>
    {
        private readonly HandlerGraph _handlers;

        public MessagingSerializationGraph(HandlerGraph handlers, IEnumerable<ISerializerFactory<IMessageDeserializer, IMessageSerializer>> serializers,
            IEnumerable<IMessageDeserializer> readers, IEnumerable<IMessageSerializer> writers)
            : base(serializers, readers, writers)
        {
            _handlers = handlers;

            // Work around here to seed this type in the serialization
            RegisterType(typeof(Acknowledgement));
        }

        public static MessagingSerializationGraph Basic()
        {
            return new MessagingSerializationGraph(new HandlerGraph(), new List<ISerializerFactory<IMessageDeserializer, IMessageSerializer>>{new NewtonsoftSerializerFactory(new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                }, new DefaultObjectPoolProvider())}, new List<IMessageDeserializer>(),
                new List<IMessageSerializer>());
        }

        protected override IEnumerable<Type> determineChainCandidates(string messageType)
        {
            return _handlers.Chains.Where(x => x.MessageType.ToMessageTypeName() == messageType)
                .Select(x => x.MessageType);
        }

        public object Deserialize(Envelope envelope)
        {
            var contentType = envelope.ContentType ?? "application/json";

            if (contentType.IsEmpty())
                throw new EnvelopeDeserializationException($"No content type can be determined for {envelope}");

            if (envelope.Data == null || envelope.Data.Length == 0)
                throw new EnvelopeDeserializationException("No data on the Envelope");

            if (envelope.MessageType.IsNotEmpty())
            {
                var reader = ReaderFor(envelope.MessageType);
                if (reader.HasAnyReaders)
                    try
                    {
                        var deserializer = reader[envelope.ContentType];
                        if (deserializer != null)
                        {
                            return deserializer.ReadFromData(envelope.Data);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw EnvelopeDeserializationException.ForReadFailure(envelope, ex);
                    }
            }

            var messageType = envelope.MessageType ?? "application/json";
            if (_serializers.ContainsKey(messageType))
                using (var stream = new MemoryStream(envelope.Data))
                {
                    stream.Position = 0;
                    return _serializers[messageType].Deserialize(stream);
                }

            throw new EnvelopeDeserializationException(
                $"Unknown content-type '{contentType}' and message-type '{envelope.MessageType}'");
        }
    }
}

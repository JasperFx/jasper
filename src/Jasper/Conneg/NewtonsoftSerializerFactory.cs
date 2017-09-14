using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Conneg
{
    // SAMPLE: NewtonsoftSerializer
    public class NewtonsoftSerializerFactory : ISerializerFactory
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;
        private readonly BusSettings _settings;

        public NewtonsoftSerializerFactory(BusSettings settings)
        {
            //settings.TypeNameHandling = TypeNameHandling.Objects;
            _serializer = Newtonsoft.Json.JsonSerializer.Create(settings.JsonSerialization);
            _settings = settings;
        }

        public void Serialize(object message, Stream stream)
        {
            var writer = new StreamWriter(stream);
            _serializer.Serialize(writer, message);
            writer.Flush();
        }

        public object Deserialize(Stream message)
        {
            var reader = new JsonTextReader(new StreamReader(message));
            return _serializer.Deserialize(reader);
        }

        public string ContentType => "application/json";

        private IEnumerable<IMessageDeserializer> determineReaders(Type messageType)
        {
            if (_settings.AllowNonVersionedSerialization)
            {
                yield return typeof(NewtonsoftJsonReader<>).CloseAndBuildAs<IMessageDeserializer>(_serializer, messageType);

                if (messageType.HasAttribute<VersionAttribute>())
                {
                    yield return VersionedReaderFor(messageType);
                }
            }
            else
            {
                yield return typeof(NewtonsoftJsonReader<>).CloseAndBuildAs<IMessageDeserializer>(messageType.ToContentType("json"), _serializer, messageType);
            }
        }

        public IMessageDeserializer[] ReadersFor(Type messageType)
        {
            return determineReaders(messageType).ToArray();
        }

        public IMessageSerializer[] WritersFor(Type messageType)
        {
            return determineWriters(messageType).ToArray();
        }

        public IMessageDeserializer VersionedReaderFor(Type incomingType)
        {
            return typeof(NewtonsoftJsonReader<>).CloseAndBuildAs<IMessageDeserializer>(incomingType.ToContentType("json"), _serializer, incomingType);
        }

        private IEnumerable<IMessageSerializer> determineWriters(Type messageType)
        {
            if (_settings.AllowNonVersionedSerialization)
            {
                yield return typeof(NewtonsoftJsonWriter<>).CloseAndBuildAs<IMessageSerializer>(_serializer, messageType);

                if (messageType.HasAttribute<VersionAttribute>())
                {
                    yield return typeof(NewtonsoftJsonWriter<>).CloseAndBuildAs<IMessageSerializer>(messageType.ToContentType("json"), _serializer, messageType);
                }
            }
            else
            {
                yield return typeof(NewtonsoftJsonWriter<>).CloseAndBuildAs<IMessageSerializer>(messageType.ToContentType("json"), _serializer, messageType);
            }
        }
    }
    // ENDSAMPLE
}

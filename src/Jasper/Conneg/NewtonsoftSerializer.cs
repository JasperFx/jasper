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
    public class NewtonsoftSerializer : ISerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;
        private readonly BusSettings _settings;

        public NewtonsoftSerializer(BusSettings settings)
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

        private IEnumerable<IMediaReader> determineReaders(Type messageType)
        {
            if (_settings.AllowNonVersionedSerialization)
            {
                yield return typeof(NewtonsoftJsonReader<>).CloseAndBuildAs<IMediaReader>(_serializer, messageType);

                if (messageType.HasAttribute<VersionAttribute>())
                {
                    yield return VersionedReaderFor(messageType);
                }
            }
            else
            {
                yield return typeof(NewtonsoftJsonReader<>).CloseAndBuildAs<IMediaReader>(messageType.ToContentType("json"), _serializer, messageType);
            }
        }

        public IMediaReader[] ReadersFor(Type messageType)
        {
            return determineReaders(messageType).ToArray();
        }

        public IMediaWriter[] WritersFor(Type messageType)
        {
            return determineWriters(messageType).ToArray();
        }

        public IMediaReader VersionedReaderFor(Type incomingType)
        {
            return typeof(NewtonsoftJsonReader<>).CloseAndBuildAs<IMediaReader>(incomingType.ToContentType("json"), _serializer, incomingType);
        }

        private IEnumerable<IMediaWriter> determineWriters(Type messageType)
        {
            if (_settings.AllowNonVersionedSerialization)
            {
                yield return typeof(NewtonsoftJsonWriter<>).CloseAndBuildAs<IMediaWriter>(_serializer, messageType);

                if (messageType.HasAttribute<VersionAttribute>())
                {
                    yield return typeof(NewtonsoftJsonWriter<>).CloseAndBuildAs<IMediaWriter>(messageType.ToContentType("json"), _serializer, messageType);
                }
            }
            else
            {
                yield return typeof(NewtonsoftJsonWriter<>).CloseAndBuildAs<IMediaWriter>(messageType.ToContentType("json"), _serializer, messageType);
            }
        }
    }
    // ENDSAMPLE
}

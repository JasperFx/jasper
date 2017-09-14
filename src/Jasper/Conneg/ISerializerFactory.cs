using System;
using System.IO;
using Baseline;
using Jasper.Bus;

namespace Jasper.Conneg
{
    // SAMPLE: ISerializer
    public interface ISerializerFactory
    {
        void Serialize(object message, Stream stream);

        object Deserialize(Stream message);

        string ContentType { get; }

        IMessageDeserializer[] ReadersFor(Type messageType);
        IMessageSerializer[] WritersFor(Type messageType);
        IMessageDeserializer VersionedReaderFor(Type incomingType);
    }
    // ENDSAMPLE
}

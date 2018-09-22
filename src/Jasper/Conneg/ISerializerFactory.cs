using System;
using System.IO;
using Baseline;

namespace Jasper.Conneg
{
    // SAMPLE: ISerializer
    public interface ISerializerFactory
    {
        object Deserialize(Stream message);

        string ContentType { get; }

        IMessageDeserializer ReaderFor(Type messageType);
        IMessageSerializer WriterFor(Type messageType);
    }
    // ENDSAMPLE
}

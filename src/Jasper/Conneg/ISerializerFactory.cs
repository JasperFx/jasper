using System;
using System.IO;

namespace Jasper.Conneg
{
    // SAMPLE: ISerializer
    public interface ISerializerFactory
    {
        string ContentType { get; }
        object Deserialize(Stream message);

        IMessageDeserializer ReaderFor(Type messageType);
        IMessageSerializer WriterFor(Type messageType);
    }

    // ENDSAMPLE
}

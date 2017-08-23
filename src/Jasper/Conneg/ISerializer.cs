using System;
using System.IO;
using Baseline;
using Jasper.Bus;

namespace Jasper.Conneg
{
    public interface ISerializer
    {
        void Serialize(object message, Stream stream);

        object Deserialize(Stream message);

        string ContentType { get; }

        IMediaReader[] ReadersFor(Type messageType);
        IMediaWriter[] WritersFor(Type messageType);
        IMediaReader VersionedReaderFor(Type incomingType);
    }
}

using System;
using System.IO;

namespace Jasper.Conneg
{
    // SAMPLE: ISerializer
    public interface ISerializerFactory<TReader, TWriter>
        where TReader : IReaderStrategy
        where TWriter : IWriterStrategy
    {
        string ContentType { get; }
        object Deserialize(Stream message);

        TReader ReaderFor(Type messageType);
        TWriter WriterFor(Type messageType);
    }

    // ENDSAMPLE
}

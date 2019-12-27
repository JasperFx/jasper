using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Util;

namespace Jasper.Serialization
{
    public interface IReaderStrategy
    {
        string MessageType { get; }
        Type DotNetType { get; }

        string ContentType { get; }
    }


    // SAMPLE: IMediaReader
    public interface IMessageDeserializer : IReaderStrategy
    {
        object ReadFromData(byte[] data);
    }
    // ENDSAMPLE

    public abstract class MessageDeserializerBase<T> : IMessageDeserializer
    {
        protected MessageDeserializerBase(string contentType)
        {
            ContentType = contentType;
            MessageType = typeof(T).ToMessageTypeName();
        }

        public string MessageType { get; }
        public Type DotNetType => typeof(T);
        public string ContentType { get; }

        public object ReadFromData(byte[] data)
        {
            return ReadData(data);
        }


        public abstract T ReadData(byte[] data);

        protected abstract Task<T> ReadData(Stream stream);
    }
}

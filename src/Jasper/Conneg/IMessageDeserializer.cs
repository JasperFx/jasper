using System;
using System.IO;
using System.Threading.Tasks;
using Baseline;
using Jasper.Util;
using Microsoft.AspNetCore.Http;

namespace Jasper.Conneg
{
    public interface IReaderStrategy
    {
        string MessageType { get; }
        Type DotNetType { get; }

        string ContentType { get; }
    }

    public interface IRequestReader : IReaderStrategy
    {
        Task<T> ReadFromRequest<T>(HttpRequest request);
    }

    // SAMPLE: IMediaReader
    public interface IMessageDeserializer : IReaderStrategy, IRequestReader
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

        public async Task<T1> ReadFromRequest<T1>(HttpRequest request)
        {
            return (await ReadData(request.Body)).As<T1>();
        }

        public abstract T ReadData(byte[] data);

        protected abstract Task<T> ReadData(Stream stream);
    }
}

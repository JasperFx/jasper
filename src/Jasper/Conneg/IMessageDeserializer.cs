using System;
using System.IO;
using System.Threading.Tasks;
using Baseline;
using Jasper.Util;
using Microsoft.AspNetCore.Http;

namespace Jasper.Conneg
{
    // SAMPLE: IMediaReader
    public interface IMessageDeserializer
    {
        string MessageType { get; }
        Type DotNetType { get; }

        string ContentType { get; }
        object ReadFromData(byte[] data);
        Task<T> ReadFromRequest<T>(HttpRequest request);
    }
    // ENDSAMPLE

    public abstract class MessageDeserializerBase<T> : IMessageDeserializer
    {
        protected MessageDeserializerBase(string contentType)
        {
            ContentType = contentType;
            MessageType = typeof(T).ToMessageAlias();
        }

        public string MessageType { get; }
        public Type DotNetType => typeof(T);
        public string ContentType { get; }

        public object ReadFromData(byte[] data)
        {
            return ReadData(data);
        }

        public abstract T ReadData(byte[] data);

        public async Task<T1> ReadFromRequest<T1>(HttpRequest request)
        {
            return (await ReadData(request.Body)).As<T1>();
        }

        protected abstract Task<T> ReadData(Stream stream);
    }
}

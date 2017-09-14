using System;
using System.Threading.Tasks;
using Baseline;
using Microsoft.AspNetCore.Http;

namespace Jasper.Conneg
{
    // SAMPLE: IMediaWriter
    public interface IMessageSerializer
    {
        Type DotNetType { get; }

        string ContentType { get; }
        byte[] Write(object model);
        Task WriteToStream(object model, HttpResponse response);
    }
    // ENDSAMPLE

    public abstract class MessageSerializerBase<T> : IMessageSerializer
    {
        public string ContentType { get; }
        public Type DotNetType { get; } = typeof(T);

        protected MessageSerializerBase(string contentType)
        {
            ContentType = contentType;
        }

        public byte[] Write(object model)
        {
            return Write(model.As<T>());
        }

        public abstract byte[] Write(T model);

        public Task WriteToStream(object model, HttpResponse response)
        {
            return WriteToStream(model.As<T>(), response);
        }

        public abstract Task WriteToStream(T model, HttpResponse response);
    }

}

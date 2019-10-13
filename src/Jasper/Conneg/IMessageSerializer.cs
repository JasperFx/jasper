using System;
using System.Threading.Tasks;
using Baseline;
using Microsoft.AspNetCore.Http;

namespace Jasper.Conneg
{
    public interface IWriterStrategy
    {
        Type DotNetType { get; }

        string ContentType { get; }
    }

    // SAMPLE: IMediaWriter
    public interface IMessageSerializer : IWriterStrategy
    {

        byte[] Write(object model);

        /// <summary>
        /// Called during HTTP requests
        /// </summary>
        /// <param name="model"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task WriteToStream(object model, HttpResponse response);
    }
    // ENDSAMPLE

    public abstract class MessageSerializerBase<T> : IMessageSerializer
    {
        protected MessageSerializerBase(string contentType)
        {
            ContentType = contentType;
        }

        public string ContentType { get; }
        public Type DotNetType { get; } = typeof(T);

        public byte[] Write(object model)
        {
            return Write(model.As<T>());
        }

        public Task WriteToStream(object model, HttpResponse response)
        {
            return WriteToStream(model.As<T>(), response);
        }

        public abstract byte[] Write(T model);

        public abstract Task WriteToStream(T model, HttpResponse response);
    }
}

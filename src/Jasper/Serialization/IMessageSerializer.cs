using System;
using Baseline;

namespace Jasper.Serialization
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

        public abstract byte[] Write(T model);

    }
}

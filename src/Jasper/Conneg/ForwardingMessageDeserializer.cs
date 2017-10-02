using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Util;
using Microsoft.AspNetCore.Http;

namespace Jasper.Conneg
{
    public class ForwardingMessageDeserializer<T> : IMessageDeserializer
    {
        private readonly IMessageDeserializer _inner;

        public ForwardingMessageDeserializer(IMessageDeserializer inner)
        {
            if (!inner.DotNetType.CanBeCastTo<IForwardsTo<T>>())
            {
                throw new ArgumentOutOfRangeException(nameof(inner), $"Inner reader type {inner.DotNetType.FullName} does not implement {typeof(IForwardsTo<T>).FullName}");
            }

            ContentType = inner.ContentType;
            _inner = inner;
        }

        public string MessageType { get; } = typeof(T).ToMessageAlias();
        public Type DotNetType { get; } = typeof(T);
        public string ContentType { get; }

        public object ReadFromData(byte[] data)
        {
            var starting = _inner.ReadFromData(data);
            return starting.As<IForwardsTo<T>>().Transform();
        }

        public async Task<T1> ReadFromRequest<T1>(HttpRequest request)
        {
            var starting = await _inner.ReadFromRequest<T1>(request);
            return starting.As<IForwardsTo<T>>().Transform().As<T1>();
        }
    }
}

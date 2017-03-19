using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JasperBus.Configuration;
using JasperBus.Runtime.Invocation;

namespace JasperBus.Runtime
{
    public interface IReceiver
    {
        Task Receive(byte[] data, IDictionary<string, string> headers, IMessageCallback callback);
    }

    public class Receiver : IReceiver
    {
        private readonly IHandlerPipeline _pipeline;
        private readonly ChannelGraph _graph;
        private readonly ChannelNode _node;
        private readonly Uri _address;

        public Receiver(IHandlerPipeline pipeline, ChannelGraph graph, ChannelNode node)
        {
            _pipeline = pipeline;
            _graph = graph;
            _node = node;
            _address = node.Uri;
        }

        public Task Receive(byte[] data, IDictionary<string, string> headers, IMessageCallback callback)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var envelope = new Envelope(data, headers, callback)
            {
                ReceivedAt = _address
            };

            // Do keep this here then.
            envelope.ContentType = envelope.ContentType ?? _node.DefaultContentType ?? _graph.DefaultContentType;

            return _pipeline.Invoke(envelope, _node);
        }
    }
}
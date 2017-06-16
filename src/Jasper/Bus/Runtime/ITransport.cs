using System;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Runtime
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        // TODO -- Make this responsible for applying the reply uri
        Task Send(Envelope envelope, Uri destination);

        /// <summary>
        /// Connect to all queues, set up incoming listeners, and tag nodes
        /// with some transport specific data
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="channels"></param>
        void Start(IHandlerPipeline pipeline, ChannelGraph channels);

        Uri DefaultReplyUri();
    }
}
using System;
using System.Collections.Generic;
using JasperBus.Configuration;
using JasperBus.Runtime.Invocation;

namespace JasperBus.Runtime
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        // TODO -- going to change this to take in an Envelope instead
        // Make this responsible for applying the reply uri
        void Send(Uri uri, byte[] data, IDictionary<string, string> headers);

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
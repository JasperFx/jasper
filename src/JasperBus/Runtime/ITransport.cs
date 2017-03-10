using System;
using JasperBus.Configuration;

namespace JasperBus.Runtime
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        /// <summary>
        /// Creates a channel for a known channel node upfront
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        IChannel CreateChannel(ChannelNode node);


        /// <summary>
        /// Creates a functional node for a channel to the reply Uri of an incoming message
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        IChannel CreateDestinationChannel(Uri destination);

        /// <summary>
        /// An address that can be used as the "callback" Uri for messages
        /// sent to a remote node
        /// </summary>
        /// <returns></returns>
        Uri ReplyUri();
    }
}
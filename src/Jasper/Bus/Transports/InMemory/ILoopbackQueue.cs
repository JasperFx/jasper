using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Transports.InMemory
{
    public interface ILoopbackQueue
    {
        void SendToReceiver(Uri destination, IReceiver receiver, LoopbackMessage message);
        void Start(IEnumerable<ChannelNode> nodes);
        Task Send(Envelope envelope, Uri destination);
        Task Send(LoopbackMessage message, Uri destination);
        Task Delay(LoopbackMessage message, Uri destination, TimeSpan delayTime);
        void ListenForMessages(ChannelNode node, IHandlerPipeline pipeline, ChannelGraph channels);
        Envelope EnvelopeForInlineMessage(object message);
    }
}
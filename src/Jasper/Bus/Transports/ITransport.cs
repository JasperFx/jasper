using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Configuration;

namespace Jasper.Bus.Transports
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        Task Send(Envelope envelope, Uri destination);

        IChannel[] Start(IHandlerPipeline pipeline, BusSettings settings, OutgoingChannels channels);

        Uri DefaultReplyUri();

        TransportState State { get; }
    }
}

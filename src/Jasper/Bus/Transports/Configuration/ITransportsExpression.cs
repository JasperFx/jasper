using System;

namespace Jasper.Bus.Transports.Configuration
{
    public interface ITransportsExpression
    {
        ITransportExpression Durable { get; }
        ITransportExpression Lightweight { get; }
        ILoopbackTransportExpression Loopback { get; }

        IQueueSettings ListenForMessagesFrom(Uri uri);
        IQueueSettings ListenForMessagesFrom(string uriString);
        void DefaultIs(string uriString);
        void DefaultIs(Uri uri);
    }
}

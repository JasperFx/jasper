using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Runtime.Routing;
using Jasper.Transports;
using Jasper.Transports.Sending;

namespace Jasper.Runtime
{
    public interface ITransportRuntime : IDisposable
    {
        ISendingAgent AddSubscriber(Uri? replyUri, ISender sender, Endpoint endpoint);
        ISendingAgent GetOrBuildSendingAgent(Uri address);
        void AddListener(IListener listener, Endpoint settings);

        IEnumerable<ISubscriber> Subscribers { get; }

        void AddSendingAgent(ISendingAgent sendingAgent);

        void AddSubscriber(ISubscriber subscriber);

        ISendingAgent AgentForLocalQueue(string queueName);
        Endpoint? EndpointFor(Uri uri);
    }
}

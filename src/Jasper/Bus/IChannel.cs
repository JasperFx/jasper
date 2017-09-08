using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jasper.Bus
{
    public interface IChannel
    {
        Uri Uri { get; }
        Uri ReplyUri { get; }

        [Obsolete("Can we get rid of this?")]
        Uri Destination { get; }

        Uri Alias { get; }

        [Obsolete("Don't think this will be necessary")]
        string QueueName();

        bool ShouldSendMessage(Type messageType);

        Task Send(Envelope envelope);
    }

}

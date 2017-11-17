using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus
{
    public interface IChannel : IDisposable
    {
        Uri Uri { get; }
        Uri LocalReplyUri { get; }

        bool ShouldSendMessage(Type messageType);

        // Rename this to FullSend?
        Task Send(Envelope envelope);

        // Resend(Envelope envelope)? Skip straight to agent?

    }

}

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

        Task Send(Envelope envelope);
    }

}

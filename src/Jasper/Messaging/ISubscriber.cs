using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging
{
    public interface ISubscriber : IDisposable
    {
        Uri Uri { get; }
        Uri ReplyUri { get; }

        bool Latched { get; }

        bool IsDurable { get; }

        int QueuedCount { get; }
        string[] ContentTypes { get; set; }

        bool ShouldSendMessage(Type messageType);

        // Rename this to FullSend?
        Task Send(Envelope envelope);


        /// <summary>
        ///     Bypasses serialization, modifiers, and persistence. Mostly used
        ///     by the outgoing "recovery" agents
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Task QuickSend(Envelope envelope);
    }
}

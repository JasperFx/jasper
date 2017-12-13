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


        /// <summary>
        /// Bypasses serialization, modifiers, and persistence. Mostly used
        /// by the outgoing "recovery" agents
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Task QuickSend(Envelope envelope);

        bool Latched { get; }

        void ApplyModifications(Envelope envelope);

        bool IsDurable { get; }
    }

}

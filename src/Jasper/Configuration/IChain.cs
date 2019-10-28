using System;
using System.Collections.Generic;
using System.Linq;
using Lamar;
using LamarCodeGeneration.Frames;

namespace Jasper.Configuration
{
    // SAMPLE: IChain
    /// <summary>
    /// Models the middleware arrangement for either an HTTP route execution
    /// or the execution of a message
    /// </summary>
    public interface IChain
    {
        /// <summary>
        /// Frames that would be initially placed in front of
        /// the primary action(s)
        /// </summary>
        IList<Frame> Middleware { get; }

        /// <summary>
        /// Frames that would be initially placed behind the primary
        /// action(s)
        /// </summary>
        IList<Frame> Postprocessors { get; }

        /// <summary>
        /// A description of this frame
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Used internally by Jasper for "outbox" mechanics
        /// </summary>
        /// <returns></returns>
        bool ShouldFlushOutgoingMessages();

        MethodCall[] HandlerCalls();
    }
    // ENDSAMPLE

}

using System;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;

namespace Jasper.Messaging.ErrorHandling
{
    // SAMPLE: IErrorHandler
    /// <summary>
    /// Potentially choose a continuation that would determine what happens next for a message
    /// that encountered the given exception at processing time
    /// </summary>
    public interface IErrorHandler
    {
        IContinuation DetermineContinuation(Envelope envelope, Exception ex);
    }
    // ENDSAMPLE
}

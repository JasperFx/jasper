using System;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
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

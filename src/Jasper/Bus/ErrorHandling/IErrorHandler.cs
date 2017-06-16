using System;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
{
    // SAMPLE: IErrorHandler
    public interface IErrorHandler
    {
        IContinuation DetermineContinuation(Envelope envelope, Exception ex);
    }
    // ENDSAMPLE
}
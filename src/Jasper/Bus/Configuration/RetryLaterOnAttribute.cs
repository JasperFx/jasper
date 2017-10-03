using System;
using Baseline.Dates;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Model;

namespace Jasper.Bus.Configuration
{
    /// <summary>
    /// Applies an error handling policy to schedule a message to be retried
    /// in a designated number of seconds after encountering the named exception
    /// </summary>
    public class RetryLaterOnAttribute : ModifyHandlerChainAttribute
    {
        private readonly Type _exceptionType;
        private readonly int _seconds;

        public RetryLaterOnAttribute(Type exceptionType, int seconds)
        {
            _exceptionType = exceptionType;
            _seconds = seconds;
        }

        public override void Modify(HandlerChain chain)
        {
            chain.OnException(_exceptionType).RetryLater(_seconds.Seconds());
        }
    }
}

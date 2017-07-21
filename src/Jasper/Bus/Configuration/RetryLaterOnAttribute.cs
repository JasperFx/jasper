using System;
using Baseline.Dates;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Model;

namespace Jasper.Bus.Configuration
{
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
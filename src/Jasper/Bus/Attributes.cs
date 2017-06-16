using System;
using Baseline.Dates;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Model;

namespace Jasper.Bus
{
    public class MaximumAttemptsAttribute : ModifyHandlerChainAttribute
    {
        private readonly int _attempts;

        public MaximumAttemptsAttribute(int attempts)
        {
            _attempts = attempts;
        }

        public override void Modify(HandlerChain chain)
        {
            chain.MaximumAttempts = _attempts;
        }
    }

    public class RetryOnAttribute : ModifyHandlerChainAttribute
    {
        private readonly Type _exceptionType;

        public RetryOnAttribute(Type exceptionType)
        {
            _exceptionType = exceptionType;
        }

        public override void Modify(HandlerChain chain)
        {
            chain.OnException(_exceptionType).Retry();
        }
    }

    public class RequeueOnAttribute : ModifyHandlerChainAttribute
    {
        private readonly Type _exceptionType;

        public RequeueOnAttribute(Type exceptionType)
        {
            _exceptionType = exceptionType;
        }

        public override void Modify(HandlerChain chain)
        {
            chain.OnException(_exceptionType).Requeue();
        }
    }

    public class MoveToErrorQueueOnAttribute : ModifyHandlerChainAttribute
    {
        private readonly Type _exceptionType;

        public MoveToErrorQueueOnAttribute(Type exceptionType)
        {
            _exceptionType = exceptionType;
        }

        public override void Modify(HandlerChain chain)
        {
            chain.OnException(_exceptionType).MoveToErrorQueue();
        }
    }

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
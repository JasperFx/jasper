using System;
using System.Linq;
using Baseline.Dates;
using Jasper.Configuration;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Model;
using LamarCodeGeneration;

namespace Jasper.Messaging.Configuration
{
    /// <summary>
    ///     Applies an error handling policy to schedule a message to be retried
    ///     in a designated number of seconds after encountering the named exception
    /// </summary>
    public class RescheduleLaterAttribute : ModifyHandlerChainAttribute
    {
        private readonly Type _exceptionType;
        private readonly int[] _seconds;

        public RescheduleLaterAttribute(Type exceptionType, params int[] seconds)
        {
            _exceptionType = exceptionType;
            _seconds = seconds;
        }

        public override void Modify(HandlerChain chain, GenerationRules rules)
        {
            chain.Retries += _exceptionType.HandledBy().Reschedule(_seconds.Select(x => x.Seconds()).ToArray());
        }
    }
}
